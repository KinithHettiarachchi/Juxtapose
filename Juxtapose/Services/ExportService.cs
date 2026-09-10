using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Juxtapose.Services
{
    public interface IExportService
    {
        string ExportToExcel(IEnumerable<ComparisonRow> rows);
        string ExportToTsv(IEnumerable<ComparisonRow> rows, string directory, string fileNamePrefix);
        string ExportSummaryTsv(IEnumerable<ComparisonRow> rows);
        string GenerateBranchAnalysisHtml(IEnumerable<ComparisonRow> rows, string leftText, string rightText);
        string BackupToTsv(IEnumerable<ComparisonRow> rows, string filePath);
        List<ComparisonRow> RestoreFromTsv(string filePath);
    }

    public class ExportService : IExportService
    {
        private static readonly string[] ColumnHeaders =
        {
            "STATUS", "LEFT", "RIGHT", "ADDED", "DELETED", "MODIFIED", "TOTAL", "CHANGE%", "REVISIONS"
        };

        public string ExportToExcel(IEnumerable<ComparisonRow> rows)
        {
            Type? excelType = Type.GetTypeFromProgID("Excel.Application");
            if (excelType == null)
            {
                throw new InvalidOperationException("Microsoft Excel is not installed.");
            }

            dynamic? excelApp = Activator.CreateInstance(excelType);
            if (excelApp == null)
            {
                throw new InvalidOperationException("Unable to start Microsoft Excel.");
            }
            excelApp.Visible = true;

            dynamic workbook = excelApp.Workbooks.Add();
            dynamic worksheet = workbook.ActiveSheet;

            for (int i = 0; i < ColumnHeaders.Length; i++)
            {
                worksheet.Cells[1, i + 1] = ColumnHeaders[i];
            }

            int rowIndex = 2;
            foreach (var row in rows.Where(r => r.Visible))
            {
                worksheet.Cells[rowIndex, 1] = row.Status;
                worksheet.Cells[rowIndex, 2] = row.Left;
                worksheet.Cells[rowIndex, 3] = row.Right;
                worksheet.Cells[rowIndex, 4] = row.Added;
                worksheet.Cells[rowIndex, 5] = row.Deleted;
                worksheet.Cells[rowIndex, 6] = row.Modified;
                worksheet.Cells[rowIndex, 7] = row.Total;
                worksheet.Cells[rowIndex, 8] = row.ChangePercent;
                worksheet.Cells[rowIndex, 9] = row.Revisions;
                rowIndex++;
            }

            worksheet.Columns.AutoFit();
            return "Exported to Excel";
        }

        public string ExportToTsv(IEnumerable<ComparisonRow> rows, string directory, string fileNamePrefix)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string timeStamp = DateTime.Now.ToString("yyMMddHHmmss");
            string filePath = Path.Combine(directory, $"{fileNamePrefix}_{timeStamp}.tsv");

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(string.Join("\t", ColumnHeaders));

                foreach (var row in rows.Where(r => r.Visible))
                {
                    writer.WriteLine(string.Join("\t", row.Status, row.Left, row.Right, row.Added, row.Deleted, row.Modified, row.Total, row.ChangePercent, row.Revisions));
                }
            }

            return filePath;
        }

        public string ExportSummaryTsv(IEnumerable<ComparisonRow> rows)
        {
            var rowsList = rows.ToList();

            var results = new List<(string Description, int Count)>
            {
                ("ADDED", CountRows(rowsList, "ADDED")),
                ("DELETED", CountRows(rowsList, "DELETED")),
                ("IDENTICAL", CountRows(rowsList, "IDENTICAL")),
                ("MODIFIED", CountRows(rowsList, "MODIFIED")),
                ("Change% >= 5% and < 25%", CountRowsWithChangePercentageRange(rowsList, 5, 25)),
                ("Change% >= 25% and <= 50%", CountRowsWithChangePercentageRange(rowsList, 25, 50)),
                ("Change% > 50%", CountRowsWithChangePercentageAbove(rowsList, 50)),
            };

            string reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(reportsDirectory))
            {
                Directory.CreateDirectory(reportsDirectory);
            }

            string timeStamp = DateTime.Now.ToString("yyMMddHHmmss");
            string tsvFilePath = Path.Combine(reportsDirectory, $"Sumart_{timeStamp}.tsv");

            using (var writer = new StreamWriter(tsvFilePath))
            {
                writer.WriteLine("Description\tCount");
                foreach (var (description, count) in results)
                {
                    writer.WriteLine($"{description}\t{count}");
                }
            }

            return tsvFilePath;
        }

        private int CountRows(List<ComparisonRow> rows, string status)
        {
            return rows.Count(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        private int CountRowsWithChangePercentageRange(List<ComparisonRow> rows, double lowerBound, double upperBound)
        {
            return rows.Count(r => double.TryParse(r.ChangePercent, out double changePercentage)
                                    && changePercentage >= lowerBound
                                    && changePercentage < upperBound);
        }

        private int CountRowsWithChangePercentageAbove(List<ComparisonRow> rows, double threshold)
        {
            return rows.Count(r => double.TryParse(r.ChangePercent, out double changePercentage)
                                    && changePercentage > threshold);
        }

        public string BackupToTsv(IEnumerable<ComparisonRow> rows, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(string.Join("\t", ColumnHeaders));

                foreach (var row in rows)
                {
                    writer.WriteLine(string.Join("\t", row.Status, row.Left, row.Right, row.Added, row.Deleted, row.Modified, row.Total, row.ChangePercent, row.Revisions));
                }
            }

            return filePath;
        }

        public List<ComparisonRow> RestoreFromTsv(string filePath)
        {
            var rows = new List<ComparisonRow>();
            var lines = File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                return rows;
            }

            foreach (var line in lines.Skip(1))
            {
                var data = line.Split('\t');
                if (data.Length < 9) continue;

                string status = data[0];
                string color = status switch
                {
                    "IDENTICAL" => "#209FF4",
                    "MODIFIED" => "#F2F249",
                    "MOVED" => "#FFA500",
                    "DELETED" => "#F2362C",
                    "ADDED" => "#73F22C",
                    _ => "#FFFFFF"
                };

                rows.Add(new ComparisonRow
                {
                    Status = status,
                    Left = data[1],
                    Right = data[2],
                    Added = data[3],
                    Deleted = data[4],
                    Modified = data[5],
                    Total = data[6],
                    ChangePercent = data[7],
                    Revisions = data[8],
                    ColorHex = color
                });
            }

            return rows;
        }

        public string GenerateBranchAnalysisHtml(IEnumerable<ComparisonRow> rows, string leftText, string rightText)
        {
            StringBuilder html = new StringBuilder();

            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='en'>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("<title>Branch Analysis</title>");
            html.AppendLine("<style>");

            html.AppendLine("body { background-color: #121212; color: white; font-family: Arial, sans-serif; }");
            html.AppendLine("table { width: 100%; border-collapse: collapse; margin-bottom: 20px; }");
            html.AppendLine("table, th, td { border: 1px solid #444; padding: 8px; text-align: left; }");
            html.AppendLine("th { background-color: #1e1e1e; cursor: pointer; }");
            html.AppendLine("tr:nth-child(even) { background-color: #333; }");
            html.AppendLine("tr:nth-child(odd) { background-color: #2e2e2e; }");
            html.AppendLine(".popup { display: none; position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%); background-color: #282828; border: 1px solid #444; padding: 20px; width: 80%; height: auto; max-height: 80%; overflow-y: auto; z-index: 1000; }");
            html.AppendLine(".popup-title { font-size: 18px; margin-bottom: 10px; color: #FFD700; }");
            html.AppendLine(".close-btn { background-color: #f44336; color: white; border: none; padding: 10px; cursor: pointer; margin-top: 10px; }");

            html.AppendLine("td.status-cell { color: white; text-align: center; }");
            html.AppendLine("td.right-align { text-align: right; }");
            html.AppendLine("td.added { background-color: #228B22; }");
            html.AppendLine("td.deleted { background-color: #B22222; }");
            html.AppendLine("td.moved { background-color: #FFD700; color: black; }");
            html.AppendLine("td.modified { background-color: #FF8C00; color: black; }");
            html.AppendLine("td.identical { background-color: #4169E1; }");
            html.AppendLine("a { color: #FF69B4; text-decoration: none; }");
            html.AppendLine("a:visited { color: #90EE90; }");

            html.AppendLine("</style>");
            html.AppendLine("<script>");

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
            html.AppendLine("    document.body.style.overflow = '';");
            html.AppendLine("    document.getElementById('popup').style.display = 'none';");
            html.AppendLine("    window.scrollTo(0, scrollY);");
            html.AppendLine("}");

            html.AppendLine("function generateRevisionTable(revisionData) {");
            html.AppendLine("    let rows = '';");
            html.AppendLine("    const revisions = revisionData.split('~~~~~~').map(r => r.trim()).filter(Boolean);");
            html.AppendLine("    revisions.forEach(revision => {");
            html.AppendLine("        const parts = revision.split(/,(?![^()]*\\))/).map(s => s.trim());");
            html.AppendLine("        if (parts.length >= 5) {");
            html.AppendLine("            const [side, rev, user, date, ...commentParts] = parts;");

            html.AppendLine("            const comment = commentParts.join(',')");
            html.AppendLine("                .replace(/(DEV-\\d+)/g, '<a href=\"https://aexis-medical.atlassian.net/browse/$1\" target=\"_blank\">$1</a>')");
            html.AppendLine("                // Replace other codes");
            html.AppendLine("                .replace(/(CB-\\d+|CW-\\d+|ER-\\d+|EX-\\d+|GEN-\\d+|GUI-\\d+|ML-\\d+|OR-\\d+|STI-\\d+)/g, '<a href=\"https://mlineteam.atlassian.net/browse/$1\" target=\"_blank\">$1</a>');");

            html.AppendLine("            rows += `<tr><td>${rev}</td><td>${user}</td><td>${date}</td><td>${comment}</td></tr>`;");
            html.AppendLine("        }");
            html.AppendLine("    });");
            html.AppendLine("    return rows;");
            html.AppendLine("}");

            html.AppendLine("function filterStatuses() {");
            html.AppendLine("const checkboxes = document.querySelectorAll('input[type=\"checkbox\"]');");
            html.AppendLine("const filters = Array.from(checkboxes).filter(checkbox => checkbox.checked).map(checkbox => checkbox.value);");
            html.AppendLine("const rows = document.querySelectorAll('#branchTable tbody tr');");
            html.AppendLine("rows.forEach(row => {");
            html.AppendLine("    const statusCell = row.querySelector('td.status-cell');");
            html.AppendLine("    const status = statusCell ? statusCell.className.split(' ')[1] : '';");
            html.AppendLine("    if (filters.length === 0 || filters.includes(status)) {");
            html.AppendLine("        row.style.display = '';");
            html.AppendLine("    } else {");
            html.AppendLine("        row.style.display = 'none';");
            html.AppendLine("    }");
            html.AppendLine("});");
            html.AppendLine("}");

            html.AppendLine("</script>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");

            html.AppendLine("<h1>Branch Analysis</h1>");
            html.AppendLine($"<h3>{leftText} vs {rightText}</h3>");
            html.AppendLine("<div>");
            html.AppendLine("<label><input type='checkbox' value='added' onclick='filterStatuses()' checked> Added</label>");
            html.AppendLine("<label><input type='checkbox' value='deleted' onclick='filterStatuses()' checked> Deleted</label>");
            html.AppendLine("<label><input type='checkbox' value='modified' onclick='filterStatuses()' checked> Modified</label>");
            html.AppendLine("<label><input type='checkbox' value='identical' onclick='filterStatuses()' checked> Identical</label>");
            html.AppendLine("<label><input type='checkbox' value='moved' onclick='filterStatuses()' checked> Moved</label>");
            html.AppendLine("</div>");

            html.AppendLine("<table id='branchTable'>");
            html.AppendLine("<thead><tr>");
            html.AppendLine("<th onclick='sortTable(0)'>STATUS</th><th onclick='sortTable(1)'>LEFT</th><th onclick='sortTable(2)'>RIGHT</th><th onclick='sortTable(3)'>ADDED</th><th onclick='sortTable(4)'>DELETED</th><th onclick='sortTable(5)'>MODIFIED</th><th onclick='sortTable(6)'>TOTAL</th><th onclick='sortTable(7)'>CHANGE%</th>");
            html.AppendLine("</tr></thead>");
            html.AppendLine("<tbody>");

            foreach (var row in rows.Where(r => r.Visible))
            {
                string status = row.Status;
                string left = row.Left;
                string right = row.Right;
                string added = row.Added;
                string deleted = row.Deleted;
                string modified = row.Modified;
                string total = row.Total;
                string changePercentage = row.ChangePercent;
                string revisions = row.Revisions;

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

            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            html.AppendLine("<div id='popup' class='popup'>");
            html.AppendLine("<div class='popup-title' id='popup-title'></div>");
            html.AppendLine("<table>");
            html.AppendLine("<thead><tr><th>Revision</th><th>User</th><th>Date</th><th>Comment</th></tr></thead>");
            html.AppendLine("<tbody id='revisionTableBody'></tbody>");
            html.AppendLine("</table>");
            html.AppendLine("<button class='close-btn' onclick='closePopup()'>Close</button>");
            html.AppendLine("</div>");

            html.AppendLine("</body>");
            html.AppendLine("</html>");

            string reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
            if (!Directory.Exists(reportsDirectory))
            {
                Directory.CreateDirectory(reportsDirectory);
            }

            string timeStamp = DateTime.Now.ToString("yyMMddHHmmss");
            string filePath = Path.Combine(reportsDirectory, $"Report_{timeStamp}.html");

            File.WriteAllText(filePath, html.ToString());

            return filePath;
        }

        private string EscapeBackslashes(string input)
        {
            return input.Replace("\\", "\\\\");
        }

        private static string GetRevisions(string revisions, bool isRight)
        {
            var revisionList = revisions.Split(new[] { "~~~~~~" }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(rev => rev.Trim())
                                        .Where(rev =>
                                        {
                                            if (isRight)
                                                return rev.StartsWith("Missing on Left");
                                            else
                                                return rev.StartsWith("Missing on Right");
                                        })
                                        .ToArray();

            var formattedRevisions = new StringBuilder();
            foreach (var revision in revisionList)
            {
                var parts = revision.Split(',').Select(p => p.Trim()).ToArray();
                if (parts.Length >= 5)
                {
                    string side = parts[0];
                    string revNumber = parts[1];
                    string user = parts[2];
                    string date = parts[3];
                    string comment = parts[4];

                    comment = comment.Replace("'", "\\'");

                    formattedRevisions.AppendLine($"~~~~~~{side}, {revNumber}, {user}, {date}, {comment}");
                }
            }

            return formattedRevisions.ToString().Trim();
        }
    }
}
