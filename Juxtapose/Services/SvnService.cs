using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Juxtapose.Services
{
    public interface ISvnService
    {
        Task<string> HandleSvnCheckoutAsync(string svnUrl, string side, bool performUpdate);
        bool IsValidSvnCheckout(string relativePath);
        Task<SvnTreeNode> BuildSvnHierarchyAsync(string svnRootUrl, int maxLevels);
        SvnTreeNode? LoadCachedSvnHierarchy();
        void OpenWinMerge(string winMergeToolPath, string leftPath, string rightPath);
        void OpenSvnDiff(string tortoiseSvnPath, string leftPath, string rightPath);
        void OpenSvnDiffInNotepad(string leftPath, string rightPath);
        void ShowSvnLog(string tortoiseSvnPath, string svnUrl);
        string GetSvnUrlFromPath(string filePath);
        string RunSvnDiffCommand(string leftFilePath, string rightFilePath);
    }

    public class SvnService : ISvnService
    {
        private readonly ILogService _log;

        public SvnService(ILogService log)
        {
            _log = log;
        }

        public async Task<string> HandleSvnCheckoutAsync(string svnUrl, string side, bool performUpdate)
        {
            _log.Log("");
            _log.Log("Handling SVN Checkout/Update...");

            string relativePath = svnUrl.Replace("svn://", "");
            string workingDir = Path.Combine(Directory.GetCurrentDirectory(), "WorkingCopy", relativePath);

            _log.Log($"    Relative path obtained : {relativePath}");
            _log.Log($"    Working directory existance checked : {workingDir}");

            if (performUpdate)
            {
                _log.Log("    SVN Update is requested.");
                if (IsValidSvnCheckout(relativePath))
                {
                    _log.Log($"The working copy for {side} already exists at {workingDir}. System will continue with SVN cleanup and update...");

                    _log.Log($"Cleaning up SVN working copy for {side} at {workingDir}...");
                    await Task.Run(() => RunSvnCommand($"svn cleanup \"{workingDir}\""));

                    _log.Log($"Updating SVN working copy for {side} at {workingDir}...");
                    await Task.Run(() => RunSvnCommand($"svn update \"{workingDir}\""));
                }
                else
                {
                    _log.Log($"The folder for {side} does not exist at {workingDir}. System will continue with SVN checkout...");

                    _log.Log($"Checking out {svnUrl} to {workingDir}...");
                    await Task.Run(() => RunSvnCommand($"svn checkout \"{svnUrl}\" \"{workingDir}\""));
                }
            }
            else
            {
                _log.Log("    Skipping SVN update as it is not requested.");
            }

            return workingDir;
        }

        public bool IsValidSvnCheckout(string relativePath)
        {
            string workingDir = Path.Combine(Directory.GetCurrentDirectory(), "WorkingCopy", relativePath);

            if (Directory.Exists(workingDir))
            {
                string svnDirectory = Path.Combine(workingDir, ".svn");
                if (Directory.Exists(svnDirectory))
                {
                    _log.Log("    The directory is a valid SVN checkout.");
                    return true;
                }
                else
                {
                    _log.Log("    The directory exists but is not a valid SVN checkout.");
                    return false;
                }
            }
            else
            {
                _log.Log("    The directory does not exist.");
                return false;
            }
        }

        private void RunSvnCommand(string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                        _log.Log(args.Data);
                };
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }
        }

        private string GetSvnFolderList(string svnUrl)
        {
            string svnListCommand = $"svn list \"{svnUrl}\" --depth immediates";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {svnListCommand}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        private static readonly string CacheFilePath = Path.Combine(AppContext.BaseDirectory, "svn-tree-cache.json");

        public async Task<SvnTreeNode> BuildSvnHierarchyAsync(string svnRootUrl, int maxLevels)
        {
            var root = new SvnTreeNode { Name = svnRootUrl };
            _log.Log($"Starting to fetch SVN hierarchy from root: {svnRootUrl}");

            await Task.Run(() => FetchSvnFoldersRecursively(root, svnRootUrl, 1, maxLevels));

            _log.Log("Completed fetching SVN hierarchy.");

            try
            {
                string json = System.Text.Json.JsonSerializer.Serialize(root);
                File.WriteAllText(CacheFilePath, json);
                _log.Log($"Saved SVN hierarchy cache to {CacheFilePath}");
            }
            catch (Exception ex)
            {
                _log.Log($"Failed to save SVN hierarchy cache: {ex.Message}");
            }

            return root;
        }

        public SvnTreeNode? LoadCachedSvnHierarchy()
        {
            try
            {
                if (!File.Exists(CacheFilePath))
                {
                    return null;
                }

                string json = File.ReadAllText(CacheFilePath);
                return System.Text.Json.JsonSerializer.Deserialize<SvnTreeNode>(json);
            }
            catch (Exception ex)
            {
                _log.Log($"Failed to load cached SVN hierarchy: {ex.Message}");
                return null;
            }
        }

        private void FetchSvnFoldersRecursively(SvnTreeNode parentNode, string currentUrl, int currentLevel, int maxLevels)
        {
            if (currentLevel > maxLevels) return;

            try
            {
                _log.Log($"Fetching folders at level {currentLevel} from {currentUrl}");

                string folderList = GetSvnFolderList(currentUrl);
                string[] folders = folderList.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string folder in folders)
                {
                    if (folder.EndsWith("/"))
                    {
                        string folderName = folder.TrimEnd('/');

                        if (folderName.Equals("IonicClient", StringComparison.OrdinalIgnoreCase) ||
                            folderName.Equals("Server", StringComparison.OrdinalIgnoreCase) ||
                            folderName.Equals("LicenceManager", StringComparison.OrdinalIgnoreCase) ||
                            folderName.Equals("launcher", StringComparison.OrdinalIgnoreCase) ||
                            folderName.Equals("Trunk", StringComparison.OrdinalIgnoreCase) ||
                            folderName.Equals("Release", StringComparison.OrdinalIgnoreCase))
                        {
                            _log.Log($"    Excluded folder: {folderName}");
                            continue;
                        }

                        var childNode = new SvnTreeNode { Name = folderName };
                        parentNode.Children.Add(childNode);

                        _log.Log($"    Added folder: {folderName} under {currentUrl}");

                        if (currentLevel == 3 && (folderName.Equals("Feature", StringComparison.OrdinalIgnoreCase) ||
                                                   folderName.Equals("Validation", StringComparison.OrdinalIgnoreCase)))
                        {
                            FetchSvnFoldersRecursively(childNode, currentUrl + "/" + folderName + "/", currentLevel + 1, maxLevels + 1);
                        }
                        else if (currentLevel == 2 && (folderName.Equals("Trunk_java11", StringComparison.OrdinalIgnoreCase) ||
                                                   folderName.Equals("Release_java11", StringComparison.OrdinalIgnoreCase)))
                        {
                            FetchSvnFoldersRecursively(childNode, currentUrl + "/" + folderName + "/", currentLevel, maxLevels - 1);
                        }
                        else
                        {
                            FetchSvnFoldersRecursively(childNode, currentUrl + "/" + folderName + "/", currentLevel + 1, maxLevels);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Log($"Error fetching folders at {currentUrl}: {ex.Message}");
            }
        }

        public void OpenWinMerge(string winMergeToolPath, string leftPath, string rightPath)
        {
            string winMergePath = $"{winMergeToolPath}\\WinMergeU.exe";

            if (File.Exists(winMergePath))
            {
                Process.Start(winMergePath, $"\"{leftPath}\" \"{rightPath}\"");
            }
            else
            {
                throw new FileNotFoundException("WinMerge is not installed at the specified path.");
            }
        }

        public void OpenSvnDiff(string tortoiseSvnPath, string leftPath, string rightPath)
        {
            string tortoiseMergePath = $"{tortoiseSvnPath}\\bin\\TortoiseMerge.exe";

            if (File.Exists(tortoiseMergePath))
            {
                Process.Start(tortoiseMergePath, $"\"{leftPath}\" \"{rightPath}\"");
            }
            else
            {
                throw new FileNotFoundException("TortoiseSVN is not installed or TortoiseMerge.exe is not found at the specified path.");
            }
        }

        public void OpenSvnDiffInNotepad(string leftPath, string rightPath)
        {
            string diffOutput = GetSvnDiffOutput(leftPath, rightPath);
            string tempFilePath = Path.Combine(Path.GetTempPath(), "SVNDiffOutput.txt");
            File.WriteAllText(tempFilePath, diffOutput);
            Process.Start("notepad.exe", tempFilePath);
        }

        private string GetSvnDiffOutput(string leftPath, string rightPath)
        {
            string leftRepoUrl = GetSvnRepositoryUrl(leftPath);
            string rightRepoUrl = GetSvnRepositoryUrl(rightPath);

            if (string.IsNullOrEmpty(leftRepoUrl) || string.IsNullOrEmpty(rightRepoUrl))
            {
                return "Error: Unable to determine SVN repository paths.";
            }

            string svnDiffCommand = $"svn diff \"{leftRepoUrl}\" \"{rightRepoUrl}\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {svnDiffCommand}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        private string GetSvnRepositoryUrl(string localPath)
        {
            string svnInfoCommand = $"svn info \"{localPath}\"";

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {svnInfoCommand}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.StartsWith("URL:"))
                    {
                        return line.Substring(5).Trim();
                    }
                }
            }

            return string.Empty;
        }

        public void ShowSvnLog(string tortoiseSvnPath, string svnUrl)
        {
            string tortoiseProcPath = $"{tortoiseSvnPath}\\bin\\TortoiseProc.exe";
            string arguments = $"/command:log /path:\"{svnUrl}\"";

            try
            {
                Process.Start(tortoiseProcPath, arguments);
            }
            catch (Exception ex)
            {
                _log.Log($"Failed to show SVN log. Error: {ex.Message}");
                throw;
            }
        }

        public string GetSvnUrlFromPath(string filePath)
        {
            Process svnProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "svn",
                    Arguments = $"info \"{filePath}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            svnProcess.Start();
            string svnInfoOutput = svnProcess.StandardOutput.ReadToEnd();
            svnProcess.WaitForExit();

            var match = Regex.Match(svnInfoOutput, @"URL:\s*(.+)");
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        public string RunSvnDiffCommand(string leftFilePath, string rightFilePath)
        {
            string leftFileUrl = GetSvnUrlFromPath(leftFilePath);
            string rightFileUrl = GetSvnUrlFromPath(rightFilePath);

            if (string.IsNullOrEmpty(leftFileUrl) || string.IsNullOrEmpty(rightFileUrl))
            {
                throw new Exception("Unable to retrieve SVN URLs for the provided file paths.");
            }

            Process svnProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "svn",
                    Arguments = $"diff \"{leftFileUrl}\" \"{rightFileUrl}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            svnProcess.Start();

            string diffOutput = svnProcess.StandardOutput.ReadToEnd();
            string errorOutput = svnProcess.StandardError.ReadToEnd();
            svnProcess.WaitForExit();

            if (!string.IsNullOrEmpty(errorOutput))
            {
                Console.WriteLine($"SVN Error: {errorOutput}");
            }

            return diffOutput;
        }
    }
}
