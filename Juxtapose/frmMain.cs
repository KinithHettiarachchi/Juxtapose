using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using Microsoft.Office.Interop.Excel;
using System.Data;
using System.Threading.Tasks;
using SharpSvn;
using System.Collections.ObjectModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Juxtapose
{
    public partial class frmMain : Form
    {
        private ContextMenuStrip contextMenu;

        public frmMain()
        {
            InitializeComponent();
            SetupFileComparisonGridView();
            // Assign the image list to the tree views
            treeLeft.ImageList = imageList1;
            treeRight.ImageList = imageList1;
        }

        private void SetupFileComparisonGridView()
        {
            gridView.Columns.Add("STATUS", "STATUS");//0
            gridView.Columns.Add("LEFT", "LEFT");//1
            gridView.Columns.Add("RIGHT", "RIGHT");//2
            gridView.Columns.Add("ADDED", "ADDED");//3
            gridView.Columns.Add("DELETED", "DELETED");//4
            gridView.Columns.Add("MODIFIED", "MODIFIED");//5
            gridView.Columns.Add("TOTAL", "TOTAL");//6
            gridView.Columns.Add("CHANGE%", "CHANGE%"); //7
            gridView.Columns.Add("REVISIONS", "REVISOINS"); //8

            gridView.Columns[0].Width = 40;
            gridView.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            gridView.Columns[1].Width = 200;
            gridView.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            gridView.Columns[2].Width = 200;
            gridView.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            gridView.Columns[3].Width = 40;
            gridView.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            gridView.Columns[4].Width = 40;
            gridView.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            gridView.Columns[5].Width = 40;
            gridView.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            gridView.Columns[6].Width = 40;
            gridView.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            gridView.Columns[7].Width = 40;
            gridView.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            gridView.Columns[8].Width = 100;
            gridView.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Create context menu
            contextMenu = new ContextMenuStrip();

            // Create the "Compare" menu and set its icon
            ToolStripMenuItem compareMenu = new ToolStripMenuItem("Compare");
            compareMenu.Image = imageList1.Images[1]; // Set image from imageList for the main menu

            // Create the "Compare with SVN..." sub-menu and set its icon
            ToolStripMenuItem compareSVNMenuItem = new ToolStripMenuItem("Compare with SVN...");
            compareSVNMenuItem.Click += CompareSVNMenuItem_Click;
            compareSVNMenuItem.Image = imageList1.Images[2]; // Set image from imageList for sub-menu
            compareMenu.DropDownItems.Add(compareSVNMenuItem);

            // Create the "Compare with SVN..." sub-menu and set its icon
            ToolStripMenuItem sohwSVNDiffMenuItem = new ToolStripMenuItem("Show SVN Diff...");
            sohwSVNDiffMenuItem.Click += ShowSVNDiffMenuItem_Click;
            sohwSVNDiffMenuItem.Image = imageList1.Images[2]; // Set image from imageList for sub-menu
            compareMenu.DropDownItems.Add(sohwSVNDiffMenuItem);

            // Create the "Compare with WinMerge..." sub-menu and set its icon
            ToolStripMenuItem compareWinMergeMenuItem = new ToolStripMenuItem("Compare with WinMerge...");
            compareWinMergeMenuItem.Click += CompareMenuItem_Click;
            compareWinMergeMenuItem.Image = imageList1.Images[3]; // Set image from imageList for sub-menu
            compareMenu.DropDownItems.Add(compareWinMergeMenuItem);

            // Add the "Compare" menu to the context menu
            contextMenu.Items.Add(compareMenu);

            // Create the "Export" menu and set its icon
            ToolStripMenuItem exportMenu = new ToolStripMenuItem("Report");
            exportMenu.Image = imageList1.Images[4]; // Set image from imageList for the main menu

            // Create the "Export to Excel..." sub-menu and set its icon
            ToolStripMenuItem exportExcelMenuItem = new ToolStripMenuItem("Export to Excel...");
            exportExcelMenuItem.Click += ExporeMenuItem_Click;
            exportExcelMenuItem.Image = imageList1.Images[5]; // Set image from imageList for sub-menu
            exportMenu.DropDownItems.Add(exportExcelMenuItem);

            // Create the "Export to HTML..." sub-menu and set its icon
            ToolStripMenuItem exportHTMLMenuItem = new ToolStripMenuItem("Export to HTML...");
            exportHTMLMenuItem.Click += ExportHTMLMenuItem_Click;
            exportHTMLMenuItem.Image = imageList1.Images[7]; // Set image from imageList for sub-menu
            exportMenu.DropDownItems.Add(exportHTMLMenuItem);

            // Create the "Export to TSV..." sub-menu and set its icon
            ToolStripMenuItem exportTSVMenuItem = new ToolStripMenuItem("Export to TSV...");
            exportTSVMenuItem.Click += ExportTSVMenuItem_Click;
            exportTSVMenuItem.Image = imageList1.Images[6]; // Set image from imageList for sub-menu
            exportMenu.DropDownItems.Add(exportTSVMenuItem);

            // Create the "Export to TSV..." sub-menu and set its icon
            ToolStripMenuItem exportTSVSymmaryMenuItem = new ToolStripMenuItem("Export Summary...");
            exportTSVSymmaryMenuItem.Click += ExportTSVSummaryMenuItem_Click;
            exportTSVSymmaryMenuItem.Image = imageList1.Images[6]; // Set image from imageList for sub-menu
            exportMenu.DropDownItems.Add(exportTSVSymmaryMenuItem);

            // Add the "Export" menu to the context menu
            contextMenu.Items.Add(exportMenu);

            // Set the context menu to the GridView
            gridView.ContextMenuStrip = contextMenu;
            gridView.MouseDown += gridView2_MouseDown;


        }

        string leftFilePath;
        string rightFilePath;
        string baseFolder;
        private void btnAnalyze_Click_1(object sender, EventArgs e)
        {
            leftFilePath = txtLeft.Text;
            rightFilePath = txtRight.Text;
            baseFolder = drpBase.Text.ToString() ?? "";
            Analzye(leftFilePath, rightFilePath, baseFolder);
        }

        private async void Analzye(string leftPath, string rightPath, string baseFolder)
        {
            LogMessage("Starting SVN update...");

            LogMessage($"    Left file path : {leftPath}");
            LogMessage($"    Right file path : {rightPath}");

            if (leftPath == null || rightPath == null || leftPath.Trim() == "" || rightPath.Trim() == "")
            {
                MessageBox.Show("The left/right paths are not defined properly!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage("Stopping SVN update as the paths are not defined properly!");
                return;
            }

            // Handle left file path (checkout or use local path)
            if (leftPath.StartsWith("svn://"))
            {
                // Perform SVN checkout or update for left path
                leftFilePath = await HandleSvnCheckout(leftPath, "left");
            }
            else
            {
                LogMessage("The left path is not a valid SVN URL!");
                MessageBox.Show("The left path is not a valid SVN URL!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Handle right file path (checkout or use local path)

            if (rightPath.StartsWith("svn://"))
            {
                // Perform SVN checkout or update for right path
                rightFilePath = await HandleSvnCheckout(rightPath, "right");
            }
            else
            {
                LogMessage("The right path is not a valid SVN URL!");
                MessageBox.Show("The right path is not a valid SVN URL!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Clear previous logs and reset progress bar
            //txtLog.Clear();
            progressBar1.Value = 0;
            LogMessage($"");
            LogMessage("Starting analysis...");

            if (leftPath == null || rightPath == null)
            {
                LogMessage("    Stopping analysis as the left path is not defined!");
                MessageBox.Show("    Stopping analysis as the left path is not defined!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Get the file lists asynchronously
            LogMessage("    Building file list for left folder...");
            var leftFileList = await Task.Run(() => GetFilteredFileList(leftFilePath + "/" + baseFolder));
            LogMessage("______________________________________________________");

            if (leftFileList == null || leftFileList.Count == 0)
            {
                LogMessage("    Stopping the analysis as the left file list is empty!");
                MessageBox.Show("    Stopping the analysis as the left file list is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LogMessage("    Building file list for right folder...");
            var rightFileList = await Task.Run(() => GetFilteredFileList(rightFilePath + "/" + baseFolder));
            LogMessage("______________________________________________________");

            if (rightFileList == null || rightFileList.Count == 0)
            {
                LogMessage("    Stopping the analysis as the left file list is empty!");
                MessageBox.Show("    Stopping the analysis as the left file list is empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Compare the file lists asynchronously
            var addedFiles = new List<string>();
            var deletedFiles = new List<string>();
            var modifiedFiles = new List<string>();
            var identicalFiles = new List<string>();
            var movedFiles = new List<string>();

            LogMessage("Comparing files...");

            var progress = new Progress<int>(value => progressBar1.Value = value);
            await Task.Run(() => CompareAndDisplayResults(leftFileList, rightFileList, addedFiles, deletedFiles, modifiedFiles, identicalFiles, movedFiles, progress, baseFolder));

            LogMessage("Analysis completed!");
            progressBar1.Value = 100;
        }

        private async Task<string?> HandleSvnCheckout(string svnUrl, string side)
        {
            LogMessage("");
            LogMessage("Handling SVN Checkout/Update...");
            // Remove 'svn://' prefix and create the checkout path in the WorkingCopy folder
            string relativePath = svnUrl.Replace("svn://", ""); // This will give us the path like 'svnserver/Client/Branches/Validation/Branch_22.2.1.1'
            string workingDir = Path.Combine(Directory.GetCurrentDirectory(), "WorkingCopy", relativePath); // Create the full working copy path

            LogMessage($"    Relative path obtained : {relativePath}");
            LogMessage($"    Working directory existance checked : {workingDir}");

            if (chkSVNUpdate.Checked == true)
            {
                LogMessage($"    SVN Update is requested.");
                if (IsValidSvnCheckout(relativePath))
                {
                    // Show confirmation to cleanup and update
                    LogMessage($"The working copy for {side} already exists at {workingDir}. System will continue with SVN cleanup and update...");

                    // Perform SVN cleanup
                    LogMessage($"Cleaning up SVN working copy for {side} at {workingDir}...");
                    await Task.Run(() => RunSvnCommand($"svn cleanup \"{workingDir}\""));

                    // Perform SVN update
                    LogMessage($"Updating SVN working copy for {side} at {workingDir}...");
                    await Task.Run(() => RunSvnCommand($"svn update \"{workingDir}\""));
                }
                else
                {
                    // Show confirmation to checkout
                    LogMessage($"The folder for {side} does not exist at {workingDir}. System will continue with SVN checkout...");

                    // Perform SVN checkout
                    LogMessage($"Checking out {svnUrl} to {workingDir}...");
                    await Task.Run(() => RunSvnCommand($"svn checkout \"{svnUrl}\" \"{workingDir}\""));
                }
            }
            else
            {
                LogMessage($"    Skipping SVN update as it is not requested.");
            }

            // Return the working directory path to be used for file listing
            return workingDir;
        }

        public bool IsValidSvnCheckout(string relativePath)
        {
            // Construct the full working copy path
            string workingDir = Path.Combine(Directory.GetCurrentDirectory(), "WorkingCopy", relativePath);

            // Check if the directory exists
            if (Directory.Exists(workingDir))
            {
                // Check if the directory contains a .svn folder, indicating a valid SVN checkout
                string svnDirectory = Path.Combine(workingDir, ".svn");
                if (Directory.Exists(svnDirectory))
                {
                    LogMessage("    The directory is a valid SVN checkout.");
                    return true; // Valid SVN checkout
                }
                else
                {
                    LogMessage("    The directory exists but is not a valid SVN checkout.");
                    return false; // Directory exists but not a valid SVN checkout
                }
            }
            else
            {
                LogMessage("    The directory does not exist.");
                return false; // Directory does not exist
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
                process.OutputDataReceived += (sender, args) => LogMessage(args.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }
        }

        private List<string>? GetFilteredFileList(string folderPath)
        {
            if (folderPath == null || folderPath == "")
            {
                return null;
            }
            // Retrieve extensions from the txtExtensions text field and split them into a list
            var extensions = txtExtensions.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                               .Select(ext => ext.Trim().ToLower())
                                               .ToList();

            LogMessage($"    Scanning folder: {folderPath}");

            List<string> fileList;

            if (extensions.Count == 0)
            {
                // If no extensions are specified, retrieve all files in the directory and subdirectories
                fileList = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories).ToList();
            }
            else
            {
                // Create a list to hold all matching files based on specified extensions
                fileList = new List<string>();

                // Loop through each extension and get the files that match
                foreach (var extension in extensions)
                {
                    var files = Directory.GetFiles(folderPath, $"*.{extension}", SearchOption.AllDirectories);
                    fileList.AddRange(files);
                }

                fileList = fileList.Distinct().ToList(); // Remove duplicates if any
            }

            LogMessage($"    Files found: {fileList.Count}");
            return fileList;
        }

        private void CompareAndDisplayResults(
            List<string> leftFileList,
            List<string> rightFileList,
            List<string> addedFiles,
            List<string> deletedFiles,
            List<string> modifiedFiles,
            List<string> identicalFiles,
            List<string> movedFiles,
            IProgress<int> progress,
            string baseFolder)
        {
            int totalFiles = leftFileList.Count + rightFileList.Count;
            int processedFiles = 0;

            // Base directory to compare relative paths
            string baseDir = $"{baseFolder}";

            // Create dictionaries for quick lookup using relative paths
            var leftFilesMap = leftFileList
                .Where(file => file.Contains(baseDir))
                .ToDictionary(file => GetRelativePath(file, baseDir), file => ComputeFileHash(file));

            var rightFilesMap = rightFileList
                .Where(file => file.Contains(baseDir))
                .ToDictionary(file => GetRelativePath(file, baseDir), file => ComputeFileHash(file));

            // Compare left side files with right side files
            foreach (var leftFile in leftFilesMap)
            {
                var relativePath = leftFile.Key;
                var fileHash = leftFile.Value;

                LogMessage($"Processing file: {relativePath}");

                if (rightFilesMap.TryGetValue(relativePath, out var rightFileHash))
                {
                    // Check if the file content is identical or modified
                    if (fileHash == rightFileHash)
                    {
                        identicalFiles.Add(relativePath);

                        // Assuming we have a method GetRevisionsForFile to get revisions
                        var revisions = GetRevisionsForFile(relativePath, "identical");
                        string revisionsString = string.Join(", ", revisions);

                        AddRowToGridView2("IDENTICAL", relativePath, relativePath, "0", "0", "0", "0", "0", "#209FF4", revisionsString);
                        LogMessage($"    IDENTICAL : {relativePath}");
                    }
                    else
                    {
                        // Calculate modified percentage and add it to modified files list
                        var (results, revisions) = CalculateModifiedPercentage(leftFilePath + "\\" + relativePath, rightFilePath + "\\" + relativePath);
                        double modifiedPercentage = results[4];
                        modifiedFiles.Add(relativePath);

                        double roundedModifiedPercentage = Math.Round(modifiedPercentage, 2);
                        string formattedPercentage = $"{roundedModifiedPercentage:F2}"; // Add percentage sign

                        // Add revisions to the display
                        string[] processedArray = revisions.Select(element => $"{element.MissingOn}, {element.Revision}, {element.Author}, {element.Time}, {element.LogMessage}").ToArray();

                        // Join the processed elements with '~~~~~~'
                        string revisionsString = string.Join("~~~~~~", processedArray);

                        // Join the elements with '~~~~~~'
                        AddRowToGridView2("MODIFIED", relativePath, relativePath, results[0].ToString(), results[1].ToString(), results[2].ToString(), results[3].ToString(), formattedPercentage, "#F2F249", revisionsString);
                        LogMessage($"    MODIFIED : {relativePath} ({modifiedPercentage:F2}) Revisions - {revisionsString}");
                    }

                    // Remove the file from the right map to track unprocessed files
                    rightFilesMap.Remove(relativePath);
                }
                else
                {
                    // Check if the file is in a different path in the right list (Moved)
                    var movedFile = rightFilesMap.Keys.FirstOrDefault(key => Path.GetFileName(key) == Path.GetFileName(relativePath));
                    if (movedFile != null)
                    {
                        movedFiles.Add($"{relativePath} -> {movedFile}"); // log both paths for clarity
                        LogMessage($"    MOVED: {relativePath} to {movedFile}");

                        // Assuming we have a method GetRevisionsForFile to get revisions
                        var revisions = GetRevisionsForFile(relativePath, "moved");
                        string revisionsString = string.Join(", ", revisions);

                        AddRowToGridView2("MOVED", relativePath, movedFile, "0", "0", "0", "0", "0", "#FFA500", revisionsString);
                    }
                    else
                    {
                        deletedFiles.Add(relativePath);

                        // Assuming we have a method GetRevisionsForFile to get revisions
                        var revisions = GetRevisionsForFile(relativePath, "deleted");
                        string revisionsString = string.Join(", ", revisions);

                        AddRowToGridView2("DELETED", relativePath, "", "0", "0", "0", "0", "0", "#F2362C", revisionsString);
                        LogMessage($"    DELETED: {relativePath}");
                    }
                }
 
                LogMessage("______________________________________________________");

                // Update progress
                processedFiles++;
                progress?.Report((processedFiles * 100) / totalFiles);

                // Scroll to the bottom of the GridView
                ScrollGridViewToBottom();
            }

            // Remaining files in rightFilesMap are the added files
            foreach (var addedFile in rightFilesMap.Keys)
            {
                addedFiles.Add(addedFile);

                // Assuming we have a method GetRevisionsForFile to get revisions
                var revisions = GetRevisionsForFile(addedFile, "added");
                string revisionsString = string.Join(", ", revisions);

                AddRowToGridView2("ADDED", "", addedFile, "0", "0", "0", "0", "0", "#73F22C", revisionsString);
                LogMessage($"    ADDED: {addedFile}");

                // Update progress
                processedFiles++;
                progress?.Report((processedFiles * 100) / totalFiles);

                // Scroll to the bottom of the GridView
                ScrollGridViewToBottom();
            }
        }

        // Method to scroll GridView to the bottom
        private void ScrollGridViewToBottom()
        {
            if (chkAutoScroll.Checked)
            {
                if (gridView.InvokeRequired)
                {
                    // We are on a different thread, so we need to invoke the UI update
                    gridView.Invoke(new MethodInvoker(ScrollGridViewToBottom));
                }
                else
                {
                    // We're on the UI thread, safe to update
                    gridView.FirstDisplayedScrollingRowIndex = gridView.RowCount - 1;
                }
            }
        }

        private List<string> GetRevisionsForFile(string filePath, string type)
        {
            List<string> revisions = new List<string>();

            // Example logic for SVN
            // Note: Ensure you have the necessary library to interact with your version control system.

            try
            {
                // Get the full path of the file
                string fullPath = Path.GetFullPath(filePath);

                // Construct the command to get revisions
                string command = $"svn log {fullPath}"; // Adjust this command as per your version control system

                // Execute the command and capture the output
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    using (var reader = process.StandardOutput)
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // Parse the line to extract revision numbers.
                            // You may need to adjust this based on how your output looks.
                            if (line.StartsWith("r")) // Revision lines usually start with 'r'
                            {
                                var parts = line.Split('|'); // Example split
                                if (parts.Length > 0)
                                {
                                    string revisionNumber = parts[0].Trim(); // Get the revision number
                                    revisions.Add(revisionNumber);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                LogMessage($"Error retrieving revisions for {filePath}: {ex.Message}");
            }

            return revisions;
        }


        // Helper method to add a row to gridView on the UI thread
        private void AddRowToGridView2(string status, string leftFile, string rightFile, string result0, string result1, string result2, string result3, string modifiedPercentage, string colorHex, string revisions)
        {
            if (gridView.InvokeRequired)
            {
                gridView.Invoke(new System.Action(() =>
                {
                    int rowIndex = gridView.Rows.Add(status, leftFile, rightFile, result0, result1, result2, result3, modifiedPercentage, revisions);
                    SetRowColor(gridView.Rows[rowIndex], ColorTranslator.FromHtml(colorHex));
                }));
            }
            else
            {
                int rowIndex = gridView.Rows.Add(status, leftFile, rightFile, result0, result1, result2, result3, modifiedPercentage, revisions);
                SetRowColor(gridView.Rows[rowIndex], ColorTranslator.FromHtml(colorHex));
            }

            LoadUsersFromRevisions();
        }


        // Helper method to get the relative path based on the base directory
        private string GetRelativePath(string fullPath, string baseDir)
        {
            int baseDirIndex = fullPath.IndexOf(baseDir, StringComparison.OrdinalIgnoreCase);
            return baseDirIndex >= 0 ? fullPath.Substring(baseDirIndex) : fullPath;
        }

        private string ComputeFileHash(string filePath)
        {
            // Compute the hash of a file to check if it has been modified or not
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

            // Declare a list to hold commit information
            List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> commits;

            // Call the comparison method and get results
            int[] result = CompareTwoFilesUsingSvn(leftFilePath, rightFilePath, out commits);

            int addedLines = 0;
            int deletedLines = 0;
            int modifiedLines = 0;
            int totalLines = 0;

            // Check for errors in the result
            if (result[0] != -1)
            {
                addedLines = result[0];
                deletedLines = result[1];
                modifiedLines = result[2];
                totalLines = result[3];

                // Calculate the modified percentage
                changePercentage = (totalLines > 0) ? (double)modifiedLines / totalLines * 100 : 0;

                // Log the results
                LogMessage($"    Modified lines: {modifiedLines}");
                LogMessage($"        (Added : {addedLines}, Deleted : {deletedLines})");
                LogMessage($"    Total lines: {totalLines}");
                LogMessage($"    Percentage of lines modified: {changePercentage:0.00}%");
            }
            else
            {
                LogMessage("An error occurred while comparing the files.");
            }

            // Prepare statistics array
            double[] statistics = new double[] { addedLines, deletedLines, modifiedLines, totalLines, changePercentage };

            // Return both statistics and the list of commits
            return (statistics, commits);
        }

        private void SetRowColor(DataGridViewRow row, Color color)
        {
            row.Cells[0].Style.BackColor = color;
        }

        private void LogMessage(string message)
        {
            // Check if we need to invoke the log message update on the UI thread
            if (txtLog.InvokeRequired)
            {
                // Invoke the method on the UI thread
                txtLog.Invoke(new Action<string>(LogMessage), message);
            }
            else
            {
                // Append log messages to the log text area with a timestamp
                txtLog.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
        }

        private void gridView2_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hitTest = gridView.HitTest(e.X, e.Y);
                if (hitTest.RowIndex >= 0)
                {
                    gridView.ClearSelection();
                    gridView.Rows[hitTest.RowIndex].Selected = true;
                    contextMenu.Show(gridView, e.Location);
                }
            }
        }

        private void CompareMenuItem_Click(object sender, EventArgs e)
        {
            if (gridView.SelectedRows.Count > 0)
            {
                var selectedRow = gridView.SelectedRows[0];
                string leftPathFull = leftFilePath + "\\" + selectedRow.Cells["Left"].Value?.ToString();
                string rightPathFull = rightFilePath + "\\" + selectedRow.Cells["Right"].Value?.ToString();

                if (!string.IsNullOrWhiteSpace(leftPathFull) && !string.IsNullOrWhiteSpace(rightPathFull))
                {
                    // Open WinMerge with the left and right paths
                    OpenWinMerge(leftPathFull, rightPathFull);
                }
                else
                {
                    MessageBox.Show("Please select valid file paths for comparison.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void CompareSVNMenuItem_Click(object sender, EventArgs e)
        {
            if (gridView.SelectedRows.Count > 0)
            {
                var selectedRow = gridView.SelectedRows[0];
                string leftPathFull = leftFilePath + "\\" + selectedRow.Cells["Left"].Value?.ToString();
                string rightPathFull = rightFilePath + "\\" + selectedRow.Cells["Right"].Value?.ToString();

                if (!string.IsNullOrWhiteSpace(leftPathFull) && !string.IsNullOrWhiteSpace(rightPathFull))
                {
                    // Open WinMerge with the left and right paths
                    OpenSvnDiff(leftPathFull, rightPathFull);
                }
                else
                {
                    MessageBox.Show("Please select valid file paths for comparison.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ExporeMenuItem_Click(object sender, EventArgs e)
        {
            ExportDataGridViewToExcel(gridView);
        }

        private void ExportTSVMenuItem_Click(object sender, EventArgs e)
        {
            ExportDataGridViewToTSV(gridView);
        }

        private void ExportTSVSummaryMenuItem_Click(object sender, EventArgs e)
        {
            CreateSummaryTSV(gridView);
        }
        private void ExportHTMLMenuItem_Click(object sender, EventArgs e)
        {
            GenerateBranchAnalysisHtml(gridView);
        }

        private void ShowSVNDiffMenuItem_Click(object sender, EventArgs e)
        {
            if (gridView.SelectedRows.Count > 0)
            {
                var selectedRow = gridView.SelectedRows[0];
                string leftPathFull = leftFilePath + "\\" + selectedRow.Cells["Left"].Value?.ToString();
                string rightPathFull = rightFilePath + "\\" + selectedRow.Cells["Right"].Value?.ToString();

                if (!string.IsNullOrWhiteSpace(leftPathFull) && !string.IsNullOrWhiteSpace(rightPathFull))
                {
                    // Get SVN diff output and open it in Notepad
                    OpenSvnDiffInNotepad(leftPathFull, rightPathFull);
                }
                else
                {
                    MessageBox.Show("Please select valid file paths for comparison.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private void ExportDataGridViewToExcel(DataGridView dgv)
        {
            Type excelType = Type.GetTypeFromProgID("Excel.Application");
            dynamic excelApp = Activator.CreateInstance(excelType);
            excelApp.Visible = true;

            dynamic workbook = excelApp.Workbooks.Add();
            dynamic worksheet = workbook.ActiveSheet;

            // Add headers
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1] = dgv.Columns[i].HeaderText;
            }

            // Add data
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                for (int j = 0; j < dgv.Columns.Count; j++)
                {
                    if (dgv.Rows[i].Visible==true && !dgv.Rows[i].IsNewRow)
                    {
                        // Check if the cell value is null and use an empty string if it is
                        worksheet.Cells[i + 2, j + 1] = dgv.Rows[i].Cells[j].Value?.ToString() ?? string.Empty;
                    }
                }
            }

            worksheet.Columns.AutoFit();
        }

        private void ExportDataGridViewToTSV(DataGridView dgv)
        {
            string reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(reportsDirectory))
            {
                Directory.CreateDirectory(reportsDirectory);
            }

            string timeStamp = DateTime.Now.ToString("yyMMddHHmmss");
            string filePath = Path.Combine(reportsDirectory, $"Report_{timeStamp}.tsv");

            // Use a StreamWriter to write the TSV file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Add headers
                for (int i = 0; i < dgv.Columns.Count; i++)
                {
                    writer.Write(dgv.Columns[i].HeaderText);
                    if (i < dgv.Columns.Count - 1)
                        writer.Write("\t"); // Add a tab separator except for the last column
                }
                writer.WriteLine(); // Move to the next line after headers

                // Add data
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    if (dgv.Rows[i].Visible == true && !dgv.Rows[i].IsNewRow) // Ignore the new row
                    {
                        for (int j = 0; j < dgv.Columns.Count; j++)
                        {
                            // Write the cell value or an empty string if the value is null
                            writer.Write(dgv.Rows[i].Cells[j].Value?.ToString() ?? string.Empty);
                            if (j < dgv.Columns.Count - 1)
                                writer.Write("\t"); // Add a tab separator except for the last column
                        }
                        writer.WriteLine(); // Move to the next line after each row
                    }
                }
            }

            // Open the TSV file using the default program
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }

        private void OpenSvnDiff(string leftPath, string rightPath)
        {
            // Specify the path to TortoiseMerge
            string tortoiseMergePath = $"{txtSVN.Text}\\bin\\TortoiseMerge.exe";

            if (File.Exists(tortoiseMergePath))
            {
                // Use TortoiseMerge to show the diff between two files
                System.Diagnostics.Process.Start(tortoiseMergePath, $"\"{leftPath}\" \"{rightPath}\"");
            }
            else
            {
                MessageBox.Show("TortoiseSVN is not installed or TortoiseMerge.exe is not found at the specified path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenSvnDiffInNotepad(string leftPath, string rightPath)
        {
            // Get the SVN diff output
            string diffOutput = GetSvnDiffOutput(leftPath, rightPath);

            // Create a temporary file to store the diff output
            string tempFilePath = Path.Combine(Path.GetTempPath(), "SVNDiffOutput.txt");

            // Write the diff output to the temporary file
            File.WriteAllText(tempFilePath, diffOutput);

            // Open the temporary file in Notepad
            Process.Start("notepad.exe", tempFilePath);
        }

        private string GetSvnDiffOutput(string leftPath, string rightPath)
        {
            // Get the SVN repository paths from the local paths
            string leftRepoUrl = GetSvnRepositoryUrl(leftPath);
            string rightRepoUrl = GetSvnRepositoryUrl(rightPath);

            if (string.IsNullOrEmpty(leftRepoUrl) || string.IsNullOrEmpty(rightRepoUrl))
            {
                return "Error: Unable to determine SVN repository paths.";
            }

            // Construct the SVN diff command using the repository URLs
            string svnDiffCommand = $"svn diff \"{leftRepoUrl}\" \"{rightRepoUrl}\"";

            // Start a process to run the SVN command
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

                // Read the output
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        private string GetSvnRepositoryUrl(string localPath)
        {
            // Construct the SVN info command
            string svnInfoCommand = $"svn info \"{localPath}\"";

            // Start a process to run the SVN info command
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

                // Read the output
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Parse the output to find the URL line
                foreach (var line in output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.StartsWith("URL:"))
                    {
                        return line.Substring(5).Trim(); // Return the URL
                    }
                }
            }

            return null; // Return null if the URL is not found
        }

        private void OpenWinMerge(string leftPath, string rightPath)
        {
            // Specify the path to WinMerge
            string winMergePath = $"{txtWinmerge.Text}\\WinMergeU.exe"; // Change this path if necessary

            if (File.Exists(winMergePath))
            {
                System.Diagnostics.Process.Start(winMergePath, $"\"{leftPath}\" \"{rightPath}\"");
            }
            else
            {
                MessageBox.Show("WinMerge is not installed at the specified path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void CreateSummaryTSV(DataGridView dataGridView)
        {
            if (dataGridView == null || dataGridView.Rows.Count == 0)
            {
                MessageBox.Show("DataGridView is empty.");
                return;
            }

            // Prepare the data for the TSV file
            var results = new System.Data.DataTable();
            results.Columns.Add("Description");
            results.Columns.Add("Count");

            // Count rows by Status
            results.Rows.Add("ADDED", CountRows(dataGridView, "ADDED"));
            results.Rows.Add("DELETED", CountRows(dataGridView, "DELETED"));
            results.Rows.Add("IDENTICAL", CountRows(dataGridView, "IDENTICAL"));
            results.Rows.Add("MODIFIED", CountRows(dataGridView, "MODIFIED"));

            // Count rows with Change% criteria
            results.Rows.Add("Change% >= 5% and < 25%", CountRowsWithChangePercentageRange(dataGridView, 5, 25));
            results.Rows.Add("Change% >= 25% and <= 50%", CountRowsWithChangePercentageRange(dataGridView, 25, 50));
            results.Rows.Add("Change% > 50%", CountRowsWithChangePercentageAbove(dataGridView, 50));

            string reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(reportsDirectory))
            {
                Directory.CreateDirectory(reportsDirectory);
            }

            string timeStamp = DateTime.Now.ToString("yyMMddHHmmss");
            string tsvFilePath = Path.Combine(reportsDirectory, $"Sumart_{timeStamp}.tsv");

            // Create TSV file
            using (var writer = new StreamWriter(tsvFilePath))
            {
                // Write the header
                writer.WriteLine("Description\tCount");

                // Write each row
                foreach (DataRow row in results.Rows)
                {
                    writer.WriteLine($"{row["Description"]}\t{row["Count"]}");
                }
            }

            // Open the TSV file
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = tsvFilePath,
                UseShellExecute = true
            });
        }

        private int CountRows(DataGridView dataGridView, string status)
        {
            return dataGridView.Rows.Cast<DataGridViewRow>()
                .Count(row => row.Cells["Status"].Value?.ToString().Equals(status, StringComparison.OrdinalIgnoreCase) == true);
        }

        private int CountRowsWithChangePercentageRange(DataGridView dataGridView, double lowerBound, double upperBound)
        {
            return dataGridView.Rows.Cast<DataGridViewRow>()
                .Count(row => double.TryParse(row.Cells["Change%"].Value?.ToString(), out double changePercentage)
                              && changePercentage >= lowerBound
                              && changePercentage < upperBound);
        }

        private int CountRowsWithChangePercentageAbove(DataGridView dataGridView, double threshold)
        {
            return dataGridView.Rows.Cast<DataGridViewRow>()
                .Count(row => double.TryParse(row.Cells["Change%"].Value?.ToString(), out double changePercentage)
                              && changePercentage > threshold);
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            drpUser.SelectedIndex = 0;
        }

        // Store the root node for shared access
        private TreeNode sharedRootNode;
        private async void PopulateTreeViewsWithSvnHierarchy(System.Windows.Forms.TreeView[] treeViews, string svnRootUrl, int maxLevels)
        {
            // Process the first tree
            var firstTreeView = treeViews[0];

            try
            {
                // Clear the first tree before populating (on UI thread)
                await InvokeOnUiThread(() => firstTreeView.Nodes.Clear());

                // Create the root node
                sharedRootNode = new TreeNode(svnRootUrl);

                // Add root node on UI thread
                await InvokeOnUiThread(() => firstTreeView.Nodes.Add(sharedRootNode));

                // Log the start of fetching
                LogMessage($"Starting to fetch SVN hierarchy from root: {svnRootUrl} for tree: {firstTreeView.Name}");

                // Fetch and populate the folder structure asynchronously
                await Task.Run(() =>
                {
                    FetchSvnFoldersRecursively(sharedRootNode, svnRootUrl, 1, maxLevels);
                });

                // Optionally expand all nodes after loading
                await InvokeOnUiThread(() => ExpandTreeViewToShowOneLevel(firstTreeView));

                // Log the completion of fetching
                LogMessage($"Completed fetching SVN hierarchy for tree: {firstTreeView.Name}.");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error fetching SVN folders for tree {firstTreeView.Name}: {ex.Message}";
                MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogMessage(errorMsg);  // Log the error as well
            }

            // Now add the same nodes to the other trees
            for (int i = 1; i < treeViews.Length; i++)
            {
                var treeView = treeViews[i];
                try
                {
                    // Clear the current tree before adding nodes (on UI thread)
                    await InvokeOnUiThread(() => treeView.Nodes.Clear());

                    // Add the shared root node and its children to the current tree
                    await InvokeOnUiThread(() => treeView.Nodes.Add((TreeNode)sharedRootNode.Clone()));

                    // Optionally expand all nodes after loading
                    await InvokeOnUiThread(() => ExpandTreeViewToShowOneLevel(treeView));
                    LogMessage($"Populated {treeView.Name} with nodes from the first tree.");
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Error adding nodes to tree {treeView.Name}: {ex.Message}";
                    MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogMessage(errorMsg);  // Log the error as well
                }
            }
        }

        // Helper method to invoke actions on the UI thread
        private Task InvokeOnUiThread(System.Action action)
        {
            return Task.Factory.StartNew(() =>
            {
                if (InvokeRequired)
                {
                    Invoke(action);
                }
                else
                {
                    action();
                }
            });
        }

        // Recursive function to fetch SVN folders and populate the TreeNode
        private void FetchSvnFoldersRecursively(TreeNode parentNode, string currentUrl, int currentLevel, int maxLevels)
        {
            if (currentLevel > maxLevels) return; // Stop if we've reached the maximum level

            try
            {
                // Log the current level and URL being fetched
                LogMessage($"Fetching folders at level {currentLevel} from {currentUrl}");

                // Get the list of folders at the current level (using --depth immediates)
                string folderList = GetSvnFolderList(currentUrl);

                // Split the output into lines
                string[] folders = folderList.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string folder in folders)
                {
                    // Only process directories (SVN list marks directories with a trailing '/')
                    if (folder.EndsWith("/"))
                    {
                        string folderName = folder.TrimEnd('/');

                        // Exclude specific folders
                        if (folderName.Equals("IonicClient", StringComparison.OrdinalIgnoreCase) ||
                            folderName.Equals("Server", StringComparison.OrdinalIgnoreCase) ||
                            folderName.Equals("LicenceManager", StringComparison.OrdinalIgnoreCase) ||
                            folderName.Equals("launcher", StringComparison.OrdinalIgnoreCase) ||
                            folderName.Equals("Trunk", StringComparison.OrdinalIgnoreCase) || // Exclude Trunk
                            folderName.Equals("Release", StringComparison.OrdinalIgnoreCase)) // Exclude Release
                        {
                            LogMessage($"    Excluded folder: {folderName}");
                            continue; // Skip the excluded folders
                        }

                        TreeNode childNode = new TreeNode(folderName);

                        // Add the child node on the UI thread
                        InvokeOnUiThread(() => parentNode.Nodes.Add(childNode));

                        // Log each added folder
                        LogMessage($"    Added folder: {folderName} under {currentUrl}");

                        // Check if we need to fetch the next level
                        if (currentLevel == 3 && (folderName.Equals("Feature", StringComparison.OrdinalIgnoreCase) ||
                                                   folderName.Equals("Validation", StringComparison.OrdinalIgnoreCase)))
                        {
                            // Get the next level (4th level)
                            FetchSvnFoldersRecursively(childNode, currentUrl + "/" + folderName + "/", currentLevel + 1, maxLevels + 1);
                        }
                        else if (currentLevel == 2 && (folderName.Equals("Trunk_java11", StringComparison.OrdinalIgnoreCase) ||
                                                   folderName.Equals("Release_java11", StringComparison.OrdinalIgnoreCase)))
                        {
                            // Get the next level (4th level)
                            FetchSvnFoldersRecursively(childNode, currentUrl + "/" + folderName + "/", currentLevel, maxLevels - 1);
                        }
                        else
                        {
                            // Recursively fetch the children of this folder if we haven't reached the max level
                            FetchSvnFoldersRecursively(childNode, currentUrl + "/" + folderName + "/", currentLevel + 1, maxLevels);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log errors encountered during the recursive fetch
                LogMessage($"Error fetching folders at {currentUrl}: {ex.Message}");
            }
        }

        // Method to fetch SVN folder list at the immediate depth
        private string GetSvnFolderList(string svnUrl)
        {
            // Construct the SVN list command for immediate depth
            string svnListCommand = $"svn list \"{svnUrl}\" --depth immediates";

            // Set up the process to execute the SVN command
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

                // Capture the output from the SVN command
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
        }

        // Method to expand TreeView to show one level
        private void ExpandTreeViewToShowOneLevel(System.Windows.Forms.TreeView treeView)
        {
            foreach (TreeNode node in treeView.Nodes)
            {
                node.Expand(); // Expand the root node
                //ExpandChildNodesToShowOneLevel(node);
            }
        }

        private void SetIconForNodeAndChildrenByIndex(TreeNode node, int imageIndex)
        {
            if (treeLeft.InvokeRequired)
            {
                // If the call is not on the UI thread, use Invoke to switch to the UI thread
                treeLeft.Invoke(new System.Action(() => SetIconForNodeAndChildrenByIndex(node, imageIndex)));
            }
            else
            {
                // Update the node icons
                node.ImageIndex = imageIndex;
                node.SelectedImageIndex = imageIndex;  // Set the selected icon as well

                // Recursively set icons for child nodes
                foreach (TreeNode childNode in node.Nodes)
                {
                    SetIconForNodeAndChildrenByIndex(childNode, imageIndex);
                }
            }
        }

        // Button click event to load SVN hierarchy for left and right trees
        private async void btnLoadSVN_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.TreeView[] trees = { treeLeft, treeRight }; // Add more trees if necessary
            await Task.Run(() => PopulateTreeViewsWithSvnHierarchy(trees, txtSVNRoot.Text, 3));
        }

        // Event handler for the AfterSelect event of treeLeft
        private void treeLeft_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Get the selected node
            TreeNode selectedNode = e.Node;

            // Build the full path from the root to the selected node, using "/" as a separator
            string fullPath = GetFullPath(selectedNode);

            // Set the full path to the txtLeft TextBox
            txtLeft.Text = fullPath;
        }

        private void treeRight_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Get the selected node
            TreeNode selectedNode = e.Node;

            // Build the full path from the root to the selected node, using "/" as a separator
            string fullPath = GetFullPath(selectedNode);

            // Set the full path to the txtLeft TextBox
            txtRight.Text = fullPath;
        }

        // Helper method to build the full path of the selected node
        private string GetFullPath(TreeNode node)
        {
            string fullPath = node.Text;

            // Traverse up the tree to build the full path
            while (node.Parent != null)
            {
                node = node.Parent;
                fullPath = node.Text + "/" + fullPath;
            }

            return fullPath;
        }

        public int[] CompareTwoFilesUsingSvn(string leftFilePath, string rightFilePath, out List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> commits)
        {
            try
            {
                // Ensure you're working with local file paths for commit history
                if (!File.Exists(leftFilePath) || !File.Exists(rightFilePath))
                {
                    throw new Exception("One or both of the provided file paths do not exist.");
                }

                // Get the list of commit information using the updated logic for differences
                commits = GetCommitHistoryForDifferences(leftFilePath, rightFilePath);

                // Now convert to SVN URLs for the diff operation
                string leftFileUrl = GetSvnUrlFromPath(leftFilePath);
                string rightFileUrl = GetSvnUrlFromPath(rightFilePath);

                if (string.IsNullOrEmpty(leftFileUrl) || string.IsNullOrEmpty(rightFileUrl))
                {
                    throw new Exception("Unable to retrieve SVN URLs for the provided file paths.");
                }

                // Run SVN diff to compare the two specified files using their URLs
                string diffOutput = RunSvnDiffCommand(leftFileUrl, rightFileUrl);

                // Parse the diff output to get the modified and total line counts
                return ParseDiffOutput(diffOutput, leftFilePath, out _);  // We no longer need the revisions from the diff output
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                commits = new List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)>(); // Initialize commits list on error
                return new int[] { -1, -1, -1, -1 }; // Return -1, -1, -1, -1 to indicate an error
            }
        }



        private List<(long Revision, string Author, DateTime Time, string LogMessage)> GetCommitHistory(string leftFileUrl, string rightFileUrl)
        {
            using (SvnClient client = new SvnClient())
            {
                // Initialize a list to hold commit details
                List<(long Revision, string Author, DateTime Time, string LogMessage)> commits = new List<(long Revision, string Author, DateTime Time, string LogMessage)>();

                // Get log for the left file
                SvnLogArgs logArgs = new SvnLogArgs
                {
                    StrictNodeHistory = true,  // Optional: Ignores merged revisions
                    Limit = 100  // Set a limit to avoid fetching too many logs
                };

                Collection<SvnLogEventArgs> leftLog;
                client.GetLog(new List<string> { leftFileUrl }, logArgs, out leftLog);

                // Get log for the right file
                Collection<SvnLogEventArgs> rightLog;
                client.GetLog(new List<string> { rightFileUrl }, logArgs, out rightLog);

                // Combine and filter unique commits from both logs
                var combinedLog = leftLog.Concat(rightLog)
                                         .DistinctBy(log => log.Revision) // Filter unique by revision number
                                         .OrderBy(log => log.Revision);

                foreach (var logEntry in combinedLog)
                {
                    commits.Add((logEntry.Revision, logEntry.Author, logEntry.Time, logEntry.LogMessage));
                }

                return commits;
            }
        }

        private List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> GetCommitHistoryForDifferences(string leftFilePath, string rightFilePath)
        {
            using (SvnClient client = new SvnClient())
            {
                // Step 1: Get the diff output between the two files
                string diffOutput = RunSvnDiffCommand(leftFilePath, rightFilePath);

                // Step 2: Identify the line numbers that differ
                var differingLines = ParseDiffForLineNumbers(diffOutput);

                // Step 3: Get the blame/annotate for the left file
                var leftBlameRevisions = GetLatestBlameRevisionsForLines(client, leftFilePath, differingLines.Left);

                // Step 4: Get the blame/annotate for the right file
                var rightBlameRevisions = GetLatestBlameRevisionsForLines(client, rightFilePath, differingLines.Right);

                // Separate the revisions that are not available on the opposite side
                var notAvailableOnLeft = rightBlameRevisions.Except(leftBlameRevisions).ToList();
                var notAvailableOnRight = leftBlameRevisions.Except(rightBlameRevisions).ToList();

                // Initialize lists for the results
                List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> commitsNotAvailableOnLeft = new();
                List<(string MissingOn, long Revision, string Author, DateTime Time, string LogMessage)> commitsNotAvailableOnRight = new();

                // Fetch logs for revisions not available on the left side
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

                // Fetch logs for revisions not available on the right side
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

                // Order by revision in descending order
                commitsNotAvailableOnLeft = commitsNotAvailableOnLeft.OrderByDescending(c => c.Revision).ToList();
                commitsNotAvailableOnRight = commitsNotAvailableOnRight.OrderByDescending(c => c.Revision).ToList();

                // Return the combined result
                return commitsNotAvailableOnLeft.Concat(commitsNotAvailableOnRight).ToList();
            }
        }

        private List<long> GetLatestBlameRevisionsForLines(SvnClient client, string filePath, List<int> lineNumbers)
        {
            string fileUrl = GetSvnUrlFromPath(filePath);

            // Get blame/annotate information for the file
            Collection<SvnBlameEventArgs> blameResults;
            client.GetBlame(new Uri(fileUrl), new SvnBlameArgs { }, out blameResults);

            // Extract the latest revision number corresponding to each specified line number
            return blameResults
                .Where(blame => lineNumbers.Contains((int)blame.LineNumber))  // Filter blame results for the specified lines
                .GroupBy(blame => blame.LineNumber)                           // Group by line number
                .Select(group => group.Max(blame => blame.Revision))          // Select the maximum (latest) revision per line
                .ToList();
        }


        private (List<int> Left, List<int> Right) ParseDiffForLineNumbers(string diffOutput)
        {
            // Lists to hold the line numbers from both left and right files
            List<int> leftLines = new List<int>();
            List<int> rightLines = new List<int>();

            // Split the diff output into lines for processing
            string[] diffLines = diffOutput.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            // Temporary variables to keep track of line numbers
            int leftLineNumber = 0;
            int rightLineNumber = 0;

            foreach (var line in diffLines)
            {
                // Check for line number indicators in the diff output
                if (line.StartsWith("@@"))
                {
                    // Extract the line numbers from the diff hunk header
                    // Example line: @@ -1,3 +1,5 @@
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var leftInfo = parts[1].Substring(1).Split(',');
                    var rightInfo = parts[2].Substring(1).Split(',');

                    // Update leftLineNumber and rightLineNumber based on the hunk header
                    leftLineNumber = int.Parse(leftInfo[0]);
                    rightLineNumber = int.Parse(rightInfo[0]);
                }
                else if (line.StartsWith("-"))
                {
                    // Line removed in the left file
                    leftLines.Add(leftLineNumber);
                    leftLineNumber++; // Increment line number for next iteration
                }
                else if (line.StartsWith("+"))
                {
                    // Line added in the right file
                    rightLines.Add(rightLineNumber);
                    rightLineNumber++; // Increment line number for next iteration
                }
                else
                {
                    // Non-diff lines (unchanged)
                    leftLineNumber++;
                    rightLineNumber++;
                }
            }

            return (Left: leftLines, Right: rightLines);
        }

        private string GetSvnUrlFromPath(string filePath)
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

            // Extract the URL from the svn info output
            var match = Regex.Match(svnInfoOutput, @"URL:\s*(.+)");
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }

        private string RunSvnDiffCommand(string leftFilePath, string rightFilePath)
        {
            // Convert local file paths to SVN URLs if necessary
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

            // Log error output if exists
            if (!string.IsNullOrEmpty(errorOutput))
            {
                Console.WriteLine($"SVN Error: {errorOutput}");
            }

            return diffOutput;
        }

        private int[] ParseDiffOutput(string diffOutput, string localFilePath, out List<string> revisions)
        {
            int addedLines = 0;
            int deletedLines = 0;
            int modifiedLines = 0;
            int totalLines = 0;

            // Use a HashSet to store unique revision numbers
            var uniqueRevisions = new HashSet<string>();

            // Regex patterns to identify added, deleted, and modified lines in the SVN diff output
            var addedLinePattern = new Regex(@"^\+", RegexOptions.Compiled);     // Lines starting with "+"
            var deletedLinePattern = new Regex(@"^-", RegexOptions.Compiled);    // Lines starting with "-"
            var revisionPattern = new Regex(@"\(revision (\d+)\)", RegexOptions.Compiled);

            // Count the total number of lines in the left file (original version)
            totalLines = CountTotalLines(localFilePath);

            using (StringReader reader = new StringReader(diffOutput))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Check for revision changes
                    if (line.Contains("(revision"))
                    {
                        // Extract revision number from the line containing the revision
                        var revisionMatch = revisionPattern.Match(line);
                        if (revisionMatch.Success)
                        {
                            uniqueRevisions.Add(revisionMatch.Groups[1].Value); // Add revision number to the HashSet
                        }
                    }

                    // Check for added lines (excluding lines with '++')
                    if (addedLinePattern.IsMatch(line) && !line.StartsWith("++"))
                    {
                        addedLines++;
                    }
                    // Check for deleted lines (excluding lines with '--')
                    else if (deletedLinePattern.IsMatch(line) && !line.StartsWith("--"))
                    {
                        deletedLines++;
                    }
                }
            }

            // Calculate modified lines (since every added/deleted line is technically modified)
            modifiedLines = addedLines + deletedLines;

            // Convert the HashSet to a List for the output parameter
            revisions = uniqueRevisions.ToList();

            // Return added, deleted, modified, and total lines as an array
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

        private void gridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // Check if the double-clicked column is either column 1 or column 2
            if (e.ColumnIndex == 1 || e.ColumnIndex == 2)
            {
                // Get the value of the cell that was double-clicked
                string cellValue = gridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();

                if (!string.IsNullOrEmpty(cellValue))
                {
                    string svnUrl = string.Empty;

                    // If the double-clicked column is 1, build the SVN URL using txtLeft.Text
                    if (e.ColumnIndex == 1)
                    {
                        svnUrl = txtLeft.Text + "/" + cellValue;
                    }
                    // If the double-clicked column is 2, build the SVN URL using txtRight.Text
                    else if (e.ColumnIndex == 2)
                    {
                        svnUrl = txtRight.Text + "/" + cellValue;
                    }

                    // Use TortoiseSVN to show the log for the constructed SVN URL
                    ShowSvnLog(svnUrl);
                }
            }
        }

        private void ShowSvnLog(string svnUrl)
        {
            // Launch TortoiseSVN's "Show Log" window for the specified SVN URL
            string tortoiseProcPath = $"{txtSVN.Text}\\bin\\TortoiseProc.exe"; // Adjust path if necessary
            string arguments = $"/command:log /path:\"{svnUrl}\"";

            try
            {
                Process.Start(tortoiseProcPath, arguments);
            }
            catch (Exception ex)
            {
                LogMessage($"Failed to show SVN log. Error: {ex.Message}");
                MessageBox.Show($"Failed to show SVN log. Error: {ex.Message}");
            }
        }

        private async void btnSVNUpdate_Click(object sender, EventArgs e)
        {

        }

        public void GenerateBranchAnalysisHtml(DataGridView gridView)
        {
            // Generate the HTML content as a string
            StringBuilder html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='en'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("<title>Branch Analysis</title>");
            html.AppendLine("<style>");

            // Dark mode CSS
            html.AppendLine("body { background-color: #121212; color: white; font-family: Arial, sans-serif; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }");
            html.AppendLine("table, th, td { border: 1px solid #444; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #1e1e1e; cursor: pointer; }");
            html.AppendLine("tr:nth-child(even) { background-color: #333; }");
            html.AppendLine("tr:nth-child(odd) { background-color: #2e2e2e; }");
            html.AppendLine(".popup { display: none; position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background-color: #282828; border: 1px solid #444; padding: 20px; width: 80%; height: auto; max-height: 80%; overflow-y: auto; z-index: 1000; }");
            html.AppendLine(".popup-title { font-size: 18px; margin-bottom: 10px; color: #FFD700; }");
            html.AppendLine(".close-btn { background-color: #f44336; color: white; border: none; padding: 10px; cursor: pointer; margin-top: 10px; }");

            // Alignment and coloring styles for STATUS column and specific columns
            html.AppendLine("td.status-cell { color: white; text-align: center; }");
            html.AppendLine("td.right-align { text-align: right; }");
            html.AppendLine("td.added { background-color: #228B22; }"); // Adjusted green
            html.AppendLine("td.deleted { background-color: #B22222; }"); // Adjusted red
            html.AppendLine("td.moved { background-color: #FFD700; color: black; }"); // Yellow for MOVED
            html.AppendLine("td.modified { background-color: #FF8C00; color: black; }"); // Orange for MODIFIED
            html.AppendLine("td.identical { background-color: #4169E1; }"); // Blue for IDENTICAL
            html.AppendLine("a { color: #FF69B4; text-decoration: none; }"); // Hyperlink color
            html.AppendLine("a:visited { color: #90EE90; }"); // Visited hyperlink color

            html.AppendLine("</style>");
            html.AppendLine("<script>");

            // Sorting function
            html.AppendLine("function sortTable(n) { /* sorting code */ }");

            html.AppendLine("function showPopup(side, revisionData) {");
            html.AppendLine("    const scrollY = window.scrollY || window.pageYOffset;");
            html.AppendLine("    document.body.dataset.scrollY = scrollY;");
            html.AppendLine("    document.body.style.overflow = 'hidden';");
            html.AppendLine("    document.getElementById('popup').style.display = 'block';");
            html.AppendLine("    document.getElementById('popup-title').textContent = side;");
            html.AppendLine("    document.getElementById('revisionTableBody').innerHTML = generateRevisionTable(revisionData);");
            html.AppendLine("    window.scrollTo(0, scrollY); ");
            html.AppendLine("}");

            html.AppendLine("function closePopup() {");
            html.AppendLine("    const scrollY = document.body.dataset.scrollY || 0;");
            html.AppendLine("    document.body.style.overflow = '';"); // Enable scrolling
            html.AppendLine("    document.getElementById('popup').style.display = 'none';");
            html.AppendLine("    window.scrollTo(0, scrollY);"); // Scroll back to the saved position
            html.AppendLine("}");

            html.AppendLine("function generateRevisionTable(revisionData) {");
            html.AppendLine("    let rows = '';");
            // Split revisionData into individual revisions and trim whitespace
            html.AppendLine("    const revisions = revisionData.split('~~~~~~').map(r => r.trim()).filter(Boolean);");
            html.AppendLine("    revisions.forEach(revision => {");
            // Split each revision into parts
            html.AppendLine("        const parts = revision.split(/,(?![^()]*\\))/).map(s => s.trim());"); // Double backslash for escaping in C#
            html.AppendLine("        if (parts.length >= 5) {");
            html.AppendLine("            const [side, rev, user, date, ...commentParts] = parts;");

            // Join comment parts and convert DEV-NUMBER and other codes to hyperlinks
            html.AppendLine("            const comment = commentParts.join(',')");
            // Replace DEV-NUMBER links
            html.AppendLine("                .replace(/(DEV-\\d+)/g, '<a href=\"https://aexis-medical.atlassian.net/browse/$1\" target=\"_blank\">$1</a>')");
            html.AppendLine("                // Replace other codes");
            html.AppendLine("                .replace(/(CB-\\d+|CW-\\d+|ER-\\d+|EX-\\d+|GEN-\\d+|GUI-\\d+|ML-\\d+|OR-\\d+|STI-\\d+)/g, '<a href=\"https://mlineteam.atlassian.net/browse/$1\" target=\"_blank\">$1</a>');");

            html.AppendLine("            rows += `<tr><td>${rev}</td><td>${user}</td><td>${date}</td><td>${comment}</td></tr>`;");
            html.AppendLine("        }");
            html.AppendLine("    });");
            html.AppendLine("    return rows;");
            html.AppendLine("}");

            // Status filtering function
            html.AppendLine("function filterStatuses() {");
            html.AppendLine("const checkboxes = document.querySelectorAll('input[type=\"checkbox\"]');");
            html.AppendLine("const filters = Array.from(checkboxes).filter(checkbox => checkbox.checked).map(checkbox => checkbox.value);");
            html.AppendLine("const rows = document.querySelectorAll('#branchTable tbody tr');");
            html.AppendLine("rows.forEach(row => {");
            html.AppendLine("    const statusCell = row.querySelector('td.status-cell');");
            html.AppendLine("    const status = statusCell ? statusCell.className.split(' ')[1] : '';"); // Get the status class (e.g., 'added')
            html.AppendLine("    if (filters.length === 0 || filters.includes(status)) {");
            html.AppendLine("        row.style.display = ''; // Show the row");
            html.AppendLine("    } else {");
            html.AppendLine("        row.style.display = 'none'; // Hide the row");
            html.AppendLine("    }");
            html.AppendLine("});");
            html.AppendLine("}");

            html.AppendLine("</script>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            // Page title and filter checkboxes
            html.AppendLine("<h1>Branch Analysis</h1>");
            html.AppendLine($"<h3>{txtLeft.Text} vs {txtRight.Text}</h3>");
            html.AppendLine("<div>");
            html.AppendLine("<label><input type='checkbox' value='added' onclick='filterStatuses()' checked> Added</label>");
            html.AppendLine("<label><input type='checkbox' value='deleted' onclick='filterStatuses()' checked> Deleted</label>");
            html.AppendLine("<label><input type='checkbox' value='modified' onclick='filterStatuses()' checked> Modified</label>");
            html.AppendLine("<label><input type='checkbox' value='identical' onclick='filterStatuses()' checked> Identical</label>");
            html.AppendLine("<label><input type='checkbox' value='moved' onclick='filterStatuses()' checked> Moved</label>");
            html.AppendLine("</div>");

            // Start the main table
            html.AppendLine("<table id='branchTable'>");
            html.AppendLine("<thead><tr>");
            html.AppendLine("<th onclick='sortTable(0)'>STATUS</th><th onclick='sortTable(1)'>LEFT</th><th onclick='sortTable(2)'>RIGHT</th><th onclick='sortTable(3)'>ADDED</th><th onclick='sortTable(4)'>DELETED</th><th onclick='sortTable(5)'>MODIFIED</th><th onclick='sortTable(6)'>TOTAL</th><th onclick='sortTable(7)'>CHANGE%</th>");
            html.AppendLine("</tr></thead>");
            html.AppendLine("<tbody>");

            // Loop through DataGridView rows and generate HTML table rows
            foreach (DataGridViewRow row in gridView.Rows)
            {
                if (row.Visible == true && !row.IsNewRow)
                {
                    string status = row.Cells["STATUS"].Value?.ToString() ?? "";
                    string left = row.Cells["LEFT"].Value?.ToString() ?? "";
                    string right = row.Cells["RIGHT"].Value?.ToString() ?? "";
                    string added = row.Cells["ADDED"].Value?.ToString() ?? "";
                    string deleted = row.Cells["DELETED"].Value?.ToString() ?? "";
                    string modified = row.Cells["MODIFIED"].Value?.ToString() ?? "";
                    string total = row.Cells["TOTAL"].Value?.ToString() ?? "";
                    string changePercentage = row.Cells["CHANGE%"].Value?.ToString() ?? "";
                    string revisions = row.Cells["REVISIONS"].Value?.ToString() ?? "";

                    string statusClass = status.ToLower() switch
                    {
                        "added" => "added",
                        "deleted" => "deleted",
                        "modified" => "modified",
                        "moved" => "moved",
                        "identical" => "identical",
                        _ => ""
                    };

                    html.AppendLine("<tr>");
                    html.AppendLine($"<td class='status-cell {statusClass}'>{status}</td>");

                    // Show hyperlink only if STATUS is 'MODIFIED'
                    if (status.ToLower() == "modified")
                    {
                        string leftRevisions = GetRevisions(revisions, false).Replace("\r\n", ". ");
                        string rightRevisions = GetRevisions(revisions, true).Replace("\r\n", ". ");

                        if (leftRevisions.Trim() != "")
                        {
                            html.AppendLine($"<td><a href='#' onclick=\"showPopup('Missing on Right : {EscapeBackslashes(left)}', '{leftRevisions}')\">{left}</a></td>");
                        }
                        else
                        {
                            html.AppendLine($"<td>{left}</td>");
                        }

                        if (rightRevisions.Trim() != "")
                        {
                            html.AppendLine($"<td><a href='#' onclick=\"showPopup('Missing on Left : {EscapeBackslashes(right)}', '{rightRevisions}')\">{right}</a></td>");
                        }
                        else
                        {
                            html.AppendLine($"<td>{right}</td>");
                        }
                    }
                    else
                    {
                        html.AppendLine($"<td>{left}</td>");
                        html.AppendLine($"<td>{right}</td>");
                    }

                    html.AppendLine($"<td class='right-align'>{added}</td>");
                    html.AppendLine($"<td class='right-align'>{deleted}</td>");
                    html.AppendLine($"<td class='right-align'>{modified}</td>");
                    html.AppendLine($"<td class='right-align'>{total}</td>");
                    html.AppendLine($"<td class='right-align'>{changePercentage}</td>");
                    html.AppendLine("</tr>");
                }
            }

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            // Popup HTML
            html.AppendLine("<div class='popup' id='popup'>");
            html.AppendLine("<div class='popup-title' id='popup-title'>Popup Title</div>");
            html.AppendLine("<table id='revisionTable'>");
            html.AppendLine("<thead><tr><th>Revision</th><th>User</th><th>Date</th><th>Comment</th></tr></thead>");
            html.AppendLine("<tbody id='revisionTableBody'></tbody>");
            html.AppendLine("</table>");
            html.AppendLine("<button class='close-btn' onclick='closePopup()'>Close</button>");
            html.AppendLine("</div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            // Write the HTML content to a file
            string reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(reportsDirectory))
            {
                Directory.CreateDirectory(reportsDirectory);
            }

            string timeStamp = DateTime.Now.ToString("yyMMddHHmmss");
            string filePath = Path.Combine(reportsDirectory, $"Report_{timeStamp}.html");

            // Save the HTML content to the file
            File.WriteAllText(filePath, html.ToString());

            // Open the file in the default browser
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
        }

        private string EscapeBackslashes(string input)
        {
            // Replace each backslash with two backslashes
            return input.Replace("\\", "\\\\");
        }

        private static string GetRevisions(string revisions, bool isRight)
        {
            // Split the revisions and filter based on the side
            var revisionList = revisions.Split(new[] { "~~~~~~" }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(rev => rev.Trim()) // Trim whitespace
                                        .Where(rev =>
                                        {
                                            // Check if the revision starts with "Missing on Left" or "Missing on Right"
                                            if (isRight)
                                                return rev.StartsWith("Missing on Left");
                                            else
                                                return rev.StartsWith("Missing on Right");
                                        })
                                        .ToArray();

            // Create a formatted string to return, escaping single quotes for JavaScript
            var formattedRevisions = new StringBuilder();
            foreach (var revision in revisionList)
            {
                var parts = revision.Split(',').Select(p => p.Trim()).ToArray();
                if (parts.Length >= 5) // Ensure there are enough parts
                {
                    string side = parts[0];
                    string revNumber = parts[1];
                    string user = parts[2];
                    string date = parts[3];
                    string comment = parts[4];

                    // Escape any single quotes for safe usage in JavaScript strings
                    comment = comment.Replace("'", "\\'");

                    // Append formatted revision string
                    formattedRevisions.AppendLine($"~~~~~~{side}, {revNumber}, {user}, {date}, {comment}");
                }
            }

            return formattedRevisions.ToString().Trim(); // Return the formatted string without extra new lines
        }

        private void txtLeft_TextChanged(object sender, EventArgs e)
        {
            string text = txtLeft.Text;

            // Clear existing items in drpBase
            drpBase.Items.Clear();

            // Check if the text contains "IonicMas"
            if (text.Contains("IonicMas"))
            {
                // Add items for "IonicMas" case
                drpBase.Items.Add("mas/src");
                drpBase.Items.Add("mas/MlineParameters");
                drpBase.Items.Add("mas/ReportMlineTemplates");
                drpBase.Items.Add("ionic-client");

                // Set the first item as the preset item
                drpBase.SelectedIndex = 0;
            }
            // Check if the text contains "Client"
            else if (text.Contains("Client"))
            {
                // Add items for "Client" case
                drpBase.Items.Add("src");
                drpBase.Items.Add("images");

                // Set the first item as the preset item
                drpBase.SelectedIndex = 0;
            }
        }

        private async void btnImport_Click(object sender, EventArgs e)
        {
            GridViewImporter importer = new GridViewImporter();
            await importer.ImportTSVToGridView(gridView);
            LoadUsersFromRevisions();
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            ExportGridViewToTSV(gridView);
        }

        public void ExportGridViewToTSV(DataGridView gridView)
        {
            // Define backup folder path
            string backupFolderPath = Path.Combine(Environment.CurrentDirectory, "Backups");

            // Ensure the Backups folder exists
            if (!Directory.Exists(backupFolderPath))
            {
                Directory.CreateDirectory(backupFolderPath);
            }

            // Prepare default filename
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string defaultFileName = $"Juxtapose_{timestamp}.tsv";

            // Set up SaveFileDialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                InitialDirectory = backupFolderPath,
                FileName = defaultFileName,
                Filter = "TSV files (*.tsv)|*.tsv",
                Title = "Save GridView Data as TSV"
            };

            // Show dialog and proceed if user clicked OK
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected file path
                string filePath = saveFileDialog.FileName;

                // Export GridView to TSV
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Write headers
                    var headers = gridView.Columns.Cast<DataGridViewColumn>().Select(column => column.HeaderText);
                    writer.WriteLine(string.Join("\t", headers));

                    // Write data rows
                    foreach (DataGridViewRow row in gridView.Rows)
                    {
                        var cells = row.Cells.Cast<DataGridViewCell>().Select(cell => cell.Value?.ToString() ?? string.Empty);
                        writer.WriteLine(string.Join("\t", cells));
                    }
                }

                // Open the folder and select the exported file
                OpenFileInExplorer(filePath);
            }
        }

        private void OpenFileInExplorer(string filePath)
        {
            // Opens Windows Explorer and selects the file
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            ApplyStatusFilterFromGridView();
        }

        private void ApplyStatusFilterFromGridView()
        {
            var selectedUser = drpUser.SelectedItem as string;

            // Collect selected statuses based on the checked checkboxes
            List<string> selectedStatuses = new List<string>();

            if (chkAdded.Checked)
                selectedStatuses.Add("ADDED");
            if (chkDeleted.Checked)
                selectedStatuses.Add("DELETED");
            if (chkMoved.Checked)
                selectedStatuses.Add("MOVED");
            if (chkModified.Checked)
                selectedStatuses.Add("MODIFIED");
            if (chkIdentical.Checked)
                selectedStatuses.Add("IDENTICAL");

            // Loop through each row in the grid view
            foreach (DataGridViewRow row in gridView.Rows)
            {
                // Get the value in the STATUS column (assuming it's the first column, index 0)
                string? statusValue = row.Cells[0].Value?.ToString();
                string? revisionsValue = row.Cells[8].Value?.ToString();

                bool isStatusMatch = statusValue != null && selectedStatuses.Contains(statusValue);
                bool isUserMatch = true; // Default to true, for "All Users"

                if (!string.IsNullOrEmpty(selectedUser) && selectedUser != "All Users")
                {
                    isUserMatch = revisionsValue != null && revisionsValue.Contains(selectedUser);
                }

                // Show the row if the status and user match the selected filters, otherwise hide it
                if (isStatusMatch && isUserMatch)
                {
                    row.Visible = true;  // Show the row
                }
                else
                {
                    row.Visible = false;  // Hide the row
                }
            }
        }

        private void LoadUsersFromRevisions()
        {
            // Ensure the UI-related code runs on the main thread
            if (drpUser.InvokeRequired)
            {
                drpUser.Invoke(new System.Action(LoadUsersFromRevisions));
                return;
            }

            // Clear existing items in the dropdown
            drpUser.Items.Clear();

            // Add "All Users" as the first and default item
            drpUser.Items.Add("All Users");

            // A HashSet is used to ensure unique user names
            HashSet<string> uniqueUsers = new HashSet<string>();

            // Loop through the rows of the gridView to get the REVISIONS column (assumed to be index 8)
            foreach (DataGridViewRow row in gridView.Rows)
            {
                if (row.Visible == true && row.Cells[8].Value != null)
                {
                    string revisions = row.Cells[8].Value.ToString();

                    // Split the revisions string into individual entries
                    string[] revisionEntries = revisions.Split(new string[] { "~~~~~~" }, StringSplitOptions.None);

                    // Variables to store the highest revision numbers for left and right lists
                    int maxLeftRevision = 0;
                    int maxRightRevision = 0;
                    string maxLeftUser = string.Empty;
                    string maxRightUser = string.Empty;

                    // Loop through each revision entry
                    foreach (var entry in revisionEntries)
                    {
                        // Split the entry by comma to extract the fields
                        string[] fields = entry.Split(',');

                        if (fields.Length >= 5)
                        {
                            string direction = fields[0].Trim();  // "Missing on Left" or "Missing on Right"
                            int revisionNumber = int.Parse(fields[1].Trim());  // Revision number
                            string userName = fields[2].Trim();  // User name

                            // Check for "Missing on Left" entries
                            if (direction.Equals("Missing on Left", StringComparison.OrdinalIgnoreCase))
                            {
                                if (revisionNumber > maxLeftRevision)
                                {
                                    maxLeftRevision = revisionNumber;
                                    maxLeftUser = userName;
                                }
                            }
                            // Check for "Missing on Right" entries
                            else if (direction.Equals("Missing on Right", StringComparison.OrdinalIgnoreCase))
                            {
                                if (revisionNumber > maxRightRevision)
                                {
                                    maxRightRevision = revisionNumber;
                                    maxRightUser = userName;
                                }
                            }
                        }
                    }

                    // Add the users with the highest revisions to the unique user list
                    if (!string.IsNullOrEmpty(maxLeftUser))
                    {
                        uniqueUsers.Add(maxLeftUser);
                    }

                    if (!string.IsNullOrEmpty(maxRightUser))
                    {
                        uniqueUsers.Add(maxRightUser);
                    }
                }
            }

            // Sort the unique users and add them to the dropdown
            var sortedUsers = uniqueUsers.OrderBy(u => u).ToList();
            foreach (var user in sortedUsers)
            {
                drpUser.Items.Add(user);
            }

            // Set "All Users" as the selected default
            drpUser.SelectedIndex = 0;
        }

    }

}
