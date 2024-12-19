using Microsoft.Office.Interop.Excel;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Juxtapose
{
    public class GridViewImporter
    {
        public async Task ImportTSVToGridView(DataGridView gridView)
        {
            // Define backup folder path
            string backupFolderPath = Path.Combine(Environment.CurrentDirectory, "Backups");

            // Ensure the Backups folder exists
            if (!Directory.Exists(backupFolderPath))
            {
                Directory.CreateDirectory(backupFolderPath);
            }

            // Set up OpenFileDialog
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = backupFolderPath,
                Filter = "TSV files (*.tsv)|*.tsv",
                Title = "Open TSV File"
            };

            // Show dialog and proceed if user clicked OK
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                // Read and load TSV data asynchronously to avoid freezing the form
                await LoadTSVIntoGridViewAsync(filePath, gridView);
            }
        }

        private async Task LoadTSVIntoGridViewAsync(string filePath, DataGridView gridView)
        {
            try
            {
                // Disable the UI while loading data
                gridView.Enabled = false;

                // Read the file asynchronously
                var lines = await File.ReadAllLinesAsync(filePath);

                if (lines.Length > 0)
                {
                    // Clear existing rows and columns in the GridView
                    gridView.Rows.Clear();
                    //gridView.Columns.Clear();

                    // Assume the first line contains headers
                    var headers = lines[0].Split('\t');
                    foreach (var header in headers)
                    {
                        //gridView.Columns.Add(header, header);
                    }

                    // Process each row starting from the second line (data rows)
                    foreach (var line in lines.Skip(1))
                    {
                        var data = line.Split('\t');
                        gridView.Rows.Add(data);
                    }

                    for (int r = 0; r < gridView.Rows.Count; r++)
                    {
                        string status= gridView.Rows[r].Cells[0].Value.ToString()??"";
                        string color="";
                        if(status == "IDENTICAL")
                        {
                            color = "#209FF4";
                        } 
                        else if (status == "MODIFIED")
                        {
                            color = "#F2F249";
                        }
                        else if (status == "MOVED")
                        {
                            color = "#FFA500";
                        }
                        else if (status == "DELETED")
                        {
                            color = "#F2362C";
                        }
                        else if (status == "ADDED")
                        {
                            color = "#73F22C";
                        }

                        gridView.Rows[r].Cells[0].Style.BackColor = ColorTranslator.FromHtml(color);
  
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading TSV file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Re-enable the GridView after loading data
                gridView.Enabled = true;
            }
        }
    }
}
