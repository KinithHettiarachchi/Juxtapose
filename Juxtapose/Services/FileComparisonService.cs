using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SharpSvn;

namespace Juxtapose.Services
{
    public class AnalysisResult
    {
        public List<ComparisonRow> Rows { get; set; } = new();
        public string LeftWorkingDir { get; set; } = string.Empty;
        public string RightWorkingDir { get; set; } = string.Empty;
    }

    public interface IFileComparisonService
    {
        Task<AnalysisResult> AnalyzeAsync(
            string leftPath,
            string rightPath,
            string baseFolder,
            string extensionsCsv,
            bool performSvnUpdate,
            IProgress<int>? progress);
    }

    public class FileComparisonService : IFileComparisonService
    {
        private readonly ILogService _log;
        private readonly ISvnService _svn;

        public FileComparisonService(ILogService log, ISvnService svn)
        {
            _log = log;
            _svn = svn;
        }

        public async Task<AnalysisResult> AnalyzeAsync(
            string leftPath,
            string rightPath,
            string baseFolder,
            string extensionsCsv,
            bool performSvnUpdate,
            IProgress<int>? progress)
        {
            _log.Log("Starting SVN update...");
            _log.Log($"    Left file path : {leftPath}");
            _log.Log($"    Right file path : {rightPath}");

            if (string.IsNullOrWhiteSpace(leftPath) || string.IsNullOrWhiteSpace(rightPath))
            {
                _log.Log("Stopping SVN update as the paths are not defined properly!");
                throw new InvalidOperationException("The left/right paths are not defined properly!");
            }

            string leftWorkingDir;
            string rightWorkingDir;

            if (leftPath.StartsWith("svn://"))
            {
                leftWorkingDir = await _svn.HandleSvnCheckoutAsync(leftPath, "left", performSvnUpdate);
            }
            else if (IsLocalAbsolutePath(leftPath))
            {
                _log.Log("    The left path is a local absolute path. Using it directly without SVN checkout.");
                if (!Directory.Exists(leftPath))
                {
                    _log.Log($"Stopping analysis as the left local path does not exist: {leftPath}");
                    throw new InvalidOperationException($"The left path does not exist: {leftPath}");
                }
                leftWorkingDir = leftPath;
            }
            else
            {
                _log.Log("The left path is not a valid SVN URL or local absolute path!");
                throw new InvalidOperationException("The left path is not a valid SVN URL or local absolute path!");
            }

            if (rightPath.StartsWith("svn://"))
            {
                rightWorkingDir = await _svn.HandleSvnCheckoutAsync(rightPath, "right", performSvnUpdate);
            }
            else if (IsLocalAbsolutePath(rightPath))
            {
                _log.Log("    The right path is a local absolute path. Using it directly without SVN checkout.");
                if (!Directory.Exists(rightPath))
                {
                    _log.Log($"Stopping analysis as the right local path does not exist: {rightPath}");
                    throw new InvalidOperationException($"The right path does not exist: {rightPath}");
                }
                rightWorkingDir = rightPath;
            }
            else
            {
                _log.Log("The right path is not a valid SVN URL or local absolute path!");
                throw new InvalidOperationException("The right path is not a valid SVN URL or local absolute path!");
            }

            _log.Log("");
            _log.Log("Starting analysis...");

            _log.Log("    Building file list for left folder...");
            var leftFileList = await Task.Run(() => GetFilteredFileList(leftWorkingDir + "/" + baseFolder, extensionsCsv));
            _log.Log("______________________________________________________");

            if (leftFileList == null || leftFileList.Count == 0)
            {
                _log.Log("    Stopping the analysis as the left file list is empty!");
                throw new InvalidOperationException("Stopping the analysis as the left file list is empty!");
            }

            _log.Log("    Building file list for right folder...");
            var rightFileList = await Task.Run(() => GetFilteredFileList(rightWorkingDir + "/" + baseFolder, extensionsCsv));
            _log.Log("______________________________________________________");

            if (rightFileList == null || rightFileList.Count == 0)
            {
                _log.Log("    Stopping the analysis as the right file list is empty!");
                throw new InvalidOperationException("Stopping the analysis as the right file list is empty!");
            }

            var rows = new List<ComparisonRow>();

            _log.Log("Comparing files...");

            await Task.Run(() => CompareAndDisplayResults(leftFileList, rightFileList, rows, progress, baseFolder, leftWorkingDir, rightWorkingDir));

            _log.Log("Analysis completed!");
            progress?.Report(100);

            return new AnalysisResult
            {
                Rows = rows,
                LeftWorkingDir = leftWorkingDir,
                RightWorkingDir = rightWorkingDir
            };
        }

        private static bool IsLocalAbsolutePath(string path)
        {
            return !string.IsNullOrWhiteSpace(path) && Path.IsPathRooted(path) && !path.StartsWith("svn://", StringComparison.OrdinalIgnoreCase);
        }

        private List<string> GetFilteredFileList(string folderPath, string extensionsCsv)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return new List<string>();
            }

            var extensions = (extensionsCsv ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                               .Select(ext => ext.Trim().ToLower())
                                               .ToList();

            _log.Log($"    Scanning folder: {folderPath}");

            List<string> fileList;

            if (extensions.Count == 0)
            {
                fileList = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories).ToList();
            }
            else
            {
                fileList = new List<string>();
                foreach (var extension in extensions)
                {
                    var files = Directory.GetFiles(folderPath, $"*.{extension}", SearchOption.AllDirectories);
                    fileList.AddRange(files);
                }
                fileList = fileList.Distinct().ToList();
            }

            _log.Log($"    Files found: {fileList.Count}");
            return fileList;
        }

        private void CompareAndDisplayResults(
            List<string> leftFileList,
            List<string> rightFileList,
            List<ComparisonRow> rows,
            IProgress<int>? progress,
            string baseFolder,
            string leftFilePath,
            string rightFilePath)
        {
            int totalFiles = leftFileList.Count + rightFileList.Count;
            int processedFiles = 0;

            string baseDir = $"{baseFolder}";

            var leftFilesMap = leftFileList
                .Where(file => file.Contains(baseDir))
                .ToDictionary(file => GetRelativePath(file, baseDir), file => ComputeFileHash(file));

            var rightFilesMap = rightFileList
                .Where(file => file.Contains(baseDir))
                .ToDictionary(file => GetRelativePath(file, baseDir), file => ComputeFileHash(file));

            foreach (var leftFile in leftFilesMap)
            {
                var relativePath = leftFile.Key;
                var fileHash = leftFile.Value;

                _log.Log($"Processing file: {relativePath}");

                if (rightFilesMap.TryGetValue(relativePath, out var rightFileHash))
                {
                    if (fileHash == rightFileHash)
                    {
                        rows.Add(new ComparisonRow
                        {
                            Status = "IDENTICAL",
                            Left = relativePath,
                            Right = relativePath,
                            ColorHex = "#209FF4",
                            Revisions = string.Empty
                        });
                        _log.Log($"    IDENTICAL : {relativePath}");
                    }
                    else
                    {
                        var (results, revisions) = CalculateModifiedPercentage(leftFilePath + "\\" + relativePath, rightFilePath + "\\" + relativePath);
                        double modifiedPercentage = results[4];

                        double roundedModifiedPercentage = Math.Round(modifiedPercentage, 2);
                        string formattedPercentage = $"{roundedModifiedPercentage:F2}";

                        string[] processedArray = revisions.Select(element => $"{element.MissingOn}, {element.Revision}, {element.Author}, {element.Time}, {element.LogMessage}").ToArray();
                        string revisionsString = string.Join("~~~~~~", processedArray);

                        rows.Add(new ComparisonRow
                        {
                            Status = "MODIFIED",
                            Left = relativePath,
                            Right = relativePath,
                            Added = results[0].ToString(),
                            Deleted = results[1].ToString(),
                            Modified = results[2].ToString(),
                            Total = results[3].ToString(),
                            ChangePercent = formattedPercentage,
                            ColorHex = "#F2F249",
                            Revisions = revisionsString
                        });
                        _log.Log($"    MODIFIED : {relativePath} ({modifiedPercentage:F2}) Revisions - {revisionsString}");
                    }

                    rightFilesMap.Remove(relativePath);
                }
                else
                {
                    var movedFile = rightFilesMap.Keys.FirstOrDefault(key => Path.GetFileName(key) == Path.GetFileName(relativePath));
                    if (movedFile != null)
                    {
                        _log.Log($"    MOVED: {relativePath} to {movedFile}");
                        rows.Add(new ComparisonRow
                        {
                            Status = "MOVED",
                            Left = relativePath,
                            Right = movedFile,
                            ColorHex = "#FFA500",
                            Revisions = string.Empty
                        });
                    }
                    else
                    {
                        rows.Add(new ComparisonRow
                        {
                            Status = "DELETED",
                            Left = relativePath,
                            Right = string.Empty,
                            ColorHex = "#F2362C",
                            Revisions = string.Empty
                        });
                        _log.Log($"    DELETED: {relativePath}");
                    }
                }

                _log.Log("______________________________________________________");

                processedFiles++;
                progress?.Report((processedFiles * 100) / totalFiles);
            }

            foreach (var addedFile in rightFilesMap.Keys)
            {
                rows.Add(new ComparisonRow
                {
                    Status = "ADDED",
                    Left = string.Empty,
                    Right = addedFile,
                    ColorHex = "#73F22C",
                    Revisions = string.Empty
                });
                _log.Log($"    ADDED: {addedFile}");

                processedFiles++;
                progress?.Report((processedFiles * 100) / totalFiles);
            }
        }

        private string GetRelativePath(string fullPath, string baseDir)
        {
            int baseDirIndex = fullPath.IndexOf(baseDir, StringComparison.OrdinalIgnoreCase);
            return baseDirIndex >= 0 ? fullPath.Substring(baseDirIndex) : fullPath;
        }

        private string ComputeFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                byte[] hashBytes = sha256.ComputeHash(fileBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public (double[] statistics, List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> commits) CalculateModifiedPercentage(string leftFilePath, string rightFilePath)
        {
            double changePercentage = 0;

            List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> commits;

            int[] result = CompareTwoFilesUsingSvn(leftFilePath, rightFilePath, out commits);

            int addedLines = 0;
            int deletedLines = 0;
            int modifiedLines = 0;
            int totalLines = 0;

            if (result[0] != -1)
            {
                addedLines = result[0];
                deletedLines = result[1];
                modifiedLines = result[2];
                totalLines = result[3];

                changePercentage = (totalLines > 0) ? (double)modifiedLines / totalLines * 100 : 0;

                _log.Log($"    Modified lines: {modifiedLines}");
                _log.Log($"        (Added : {addedLines}, Deleted : {deletedLines})");
                _log.Log($"    Total lines: {totalLines}");
                _log.Log($"    Percentage of lines modified: {changePercentage:0.00}%");
            }
            else
            {
                _log.Log("An error occurred while comparing the files.");
            }

            double[] statistics = new double[] { addedLines, deletedLines, modifiedLines, totalLines, changePercentage };

            return (statistics, commits);
        }

        public int[] CompareTwoFilesUsingSvn(string leftFilePath, string rightFilePath, out List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> commits)
        {
            try
            {
                if (!File.Exists(leftFilePath) || !File.Exists(rightFilePath))
                {
                    throw new Exception("One or both of the provided file paths do not exist.");
                }

                commits = GetCommitHistoryForDifferences(leftFilePath, rightFilePath);

                string leftFileUrl = _svn.GetSvnUrlFromPath(leftFilePath);
                string rightFileUrl = _svn.GetSvnUrlFromPath(rightFilePath);

                if (string.IsNullOrEmpty(leftFileUrl) || string.IsNullOrEmpty(rightFileUrl))
                {
                    throw new Exception("Unable to retrieve SVN URLs for the provided file paths.");
                }

                string diffOutput = _svn.RunSvnDiffCommand(leftFileUrl, rightFileUrl);

                return ParseDiffOutput(diffOutput, leftFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                commits = new List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)>();
                return new int[] { -1, -1, -1, -1 };
            }
        }

        private List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> GetCommitHistoryForDifferences(string leftFilePath, string rightFilePath)
        {
            using (SvnClient client = new SvnClient())
            {
                string diffOutput = _svn.RunSvnDiffCommand(leftFilePath, rightFilePath);

                var differingLines = ParseDiffForLineNumbers(diffOutput);

                var leftBlameRevisions = GetLatestBlameRevisionsForLines(client, leftFilePath, differingLines.Left);
                var rightBlameRevisions = GetLatestBlameRevisionsForLines(client, rightFilePath, differingLines.Right);

                var notAvailableOnLeft = rightBlameRevisions.Except(leftBlameRevisions).ToList();
                var notAvailableOnRight = leftBlameRevisions.Except(rightBlameRevisions).ToList();

                List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> commitsNotAvailableOnLeft = new();
                List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> commitsNotAvailableOnRight = new();

                foreach (var revision in notAvailableOnLeft)
                {
                    SvnRevisionRange revisionRange = new SvnRevisionRange(revision, revision);
                    client.GetLog(new List<string> { rightFilePath }, new SvnLogArgs { Range = revisionRange }, out var logEntries);
                    foreach (var logEntry in logEntries)
                    {
                        commitsNotAvailableOnLeft.Add((
                                    "Missing on Left",
                                    logEntry.Revision,
                                    logEntry.Author ?? "",
                                    logEntry.Time,
                                    logEntry.LogMessage?.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ") ?? ""
                        ));
                    }
                }

                foreach (var revision in notAvailableOnRight)
                {
                    SvnRevisionRange revisionRange = new SvnRevisionRange(revision, revision);
                    client.GetLog(new List<string> { leftFilePath }, new SvnLogArgs { Range = revisionRange }, out var logEntries);
                    foreach (var logEntry in logEntries)
                    {
                        commitsNotAvailableOnRight.Add((
                                    "Missing on Right",
                                    logEntry.Revision,
                                    logEntry.Author ?? "",
                                    logEntry.Time,
                                    logEntry.LogMessage?.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ") ?? ""
                        ));
                    }
                }

                commitsNotAvailableOnLeft = commitsNotAvailableOnLeft.OrderByDescending(c => c.Revision).ToList();
                commitsNotAvailableOnRight = commitsNotAvailableOnRight.OrderByDescending(c => c.Revision).ToList();

                return commitsNotAvailableOnLeft.Concat(commitsNotAvailableOnRight).ToList();
            }
        }

        private List<long> GetLatestBlameRevisionsForLines(SvnClient client, string filePath, List<int> lineNumbers)
        {
            string fileUrl = _svn.GetSvnUrlFromPath(filePath);

            Collection<SvnBlameEventArgs> blameResults;
            client.GetBlame(new Uri(fileUrl), new SvnBlameArgs { }, out blameResults);

            return blameResults
                .Where(blame => lineNumbers.Contains((int)blame.LineNumber))
                .GroupBy(blame => blame.LineNumber)
                .Select(group => group.Max(blame => blame.Revision))
                .ToList();
        }

        private (List<int> Left, List<int> Right) ParseDiffForLineNumbers(string diffOutput)
        {
            List<int> leftLines = new List<int>();
            List<int> rightLines = new List<int>();

            string[] diffLines = diffOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            int leftLineNumber = 0;
            int rightLineNumber = 0;

            foreach (var line in diffLines)
            {
                if (line.StartsWith("@@"))
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var leftInfo = parts[1].Substring(1).Split(',');
                    var rightInfo = parts[2].Substring(1).Split(',');

                    leftLineNumber = int.Parse(leftInfo[0]);
                    rightLineNumber = int.Parse(rightInfo[0]);
                }
                else if (line.StartsWith("-"))
                {
                    leftLines.Add(leftLineNumber);
                    leftLineNumber++;
                }
                else if (line.StartsWith("+"))
                {
                    rightLines.Add(rightLineNumber);
                    rightLineNumber++;
                }
                else
                {
                    leftLineNumber++;
                    rightLineNumber++;
                }
            }

            return (Left: leftLines, Right: rightLines);
        }

        private int[] ParseDiffOutput(string diffOutput, string localFilePath)
        {
            int addedLines = 0;
            int deletedLines = 0;
            int modifiedLines;
            int totalLines;

            var addedLinePattern = new System.Text.RegularExpressions.Regex(@"^\+", System.Text.RegularExpressions.RegexOptions.Compiled);
            var deletedLinePattern = new System.Text.RegularExpressions.Regex(@"^-", System.Text.RegularExpressions.RegexOptions.Compiled);

            totalLines = CountTotalLines(localFilePath);

            using (StringReader reader = new StringReader(diffOutput))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (addedLinePattern.IsMatch(line) && !line.StartsWith("++"))
                    {
                        addedLines++;
                    }
                    else if (deletedLinePattern.IsMatch(line) && !line.StartsWith("--"))
                    {
                        deletedLines++;
                    }
                }
            }

            modifiedLines = addedLines + deletedLines;

            return new int[] { addedLines, deletedLines, modifiedLines, totalLines };
        }

        private int CountTotalLines(string filePath)
        {
            int lineCount = 0;
            using (StreamReader reader = new StreamReader(filePath))
            {
                while (reader.ReadLine() != null)
                {
                    lineCount++;
                }
            }
            return lineCount;
        }
    }
}
