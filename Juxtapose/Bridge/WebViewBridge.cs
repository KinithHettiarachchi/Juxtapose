using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Juxtapose.Services;

namespace Juxtapose.Bridge
{
    /// <summary>
    /// JSON-RPC style bridge that exposes the backend services to the WebView2 hosted UI.
    /// Every request coming from JavaScript is a JSON envelope: { requestId, action, payload }.
    /// Every response sent back is: { requestId, success, data, error }.
    /// Unsolicited events (log lines, progress) are pushed as: { type: "log"|"progress", ... }.
    /// </summary>
    public class WebViewBridge
    {
        private readonly ILogService _logService;
        private readonly ISvnService _svnService;
        private readonly IFileComparisonService _fileComparisonService;
        private readonly IExportService _exportService;
        private readonly Action<string> _postMessage;

        private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public WebViewBridge(
            ILogService logService,
            ISvnService svnService,
            IFileComparisonService fileComparisonService,
            IExportService exportService,
            Action<string> postMessage)
        {
            _logService = logService;
            _svnService = svnService;
            _fileComparisonService = fileComparisonService;
            _exportService = exportService;
            _postMessage = postMessage;

            _logService.MessageLogged += OnMessageLogged;
        }

        private void OnMessageLogged(string message)
        {
            _postMessage(JsonSerializer.Serialize(new { type = "log", message }, s_jsonOptions));
        }

        private void PostProgress(int percent)
        {
            _postMessage(JsonSerializer.Serialize(new { type = "progress", percent }, s_jsonOptions));
        }

        public async Task<string> HandleMessageAsync(string json)
        {
            string? requestId = null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                requestId = root.TryGetProperty("requestId", out var idEl) ? idEl.GetString() : null;
                string action = root.GetProperty("action").GetString() ?? string.Empty;
                JsonElement payload = root.TryGetProperty("payload", out var p) ? p : default;

                object? data = action switch
                {
                    "analyze" => await HandleAnalyzeAsync(payload),
                    "loadSvnHierarchy" => await HandleLoadSvnHierarchyAsync(payload),
                    "loadCachedSvnHierarchy" => HandleLoadCachedSvnHierarchy(payload),
                    "backup" => HandleBackup(payload),
                    "restore" => HandleRestore(payload),
                    "exportExcel" => HandleExportExcel(payload),
                    "exportTsv" => HandleExportTsv(payload),
                    "exportSummaryTsv" => HandleExportSummaryTsv(payload),
                    "exportHtml" => HandleExportHtml(payload),
                    "openWinMerge" => HandleOpenWinMerge(payload),
                    "openSvnDiff" => HandleOpenSvnDiff(payload),
                    "openSvnDiffNotepad" => HandleOpenSvnDiffNotepad(payload),
                    "showSvnLog" => HandleShowSvnLog(payload),
                    "pickOpenFile" => HandlePickOpenFile(payload),
                    "pickSaveFile" => HandlePickSaveFile(payload),
                    _ => throw new InvalidOperationException($"Unknown action '{action}'"),
                };

                return JsonSerializer.Serialize(new BridgeResponse
                {
                    RequestId = requestId,
                    Success = true,
                    Data = data
                }, s_jsonOptions);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new BridgeResponse
                {
                    RequestId = requestId,
                    Success = false,
                    Error = ex.Message
                }, s_jsonOptions);
            }
        }

        private async Task<object?> HandleAnalyzeAsync(JsonElement payload)
        {
            string leftPath = GetString(payload, "leftPath");
            string rightPath = GetString(payload, "rightPath");
            string baseFolder = GetString(payload, "baseFolder");
            string extensionsCsv = GetString(payload, "extensionsCsv");
            bool performSvnUpdate = GetBool(payload, "performSvnUpdate");

            var progress = new Progress<int>(PostProgress);

            AnalysisResult result = await _fileComparisonService.AnalyzeAsync(
                leftPath, rightPath, baseFolder, extensionsCsv, performSvnUpdate, progress);

            return result;
        }

        private async Task<object?> HandleLoadSvnHierarchyAsync(JsonElement payload)
        {
            string svnRootUrl = GetString(payload, "svnRootUrl");
            int maxLevels = payload.TryGetProperty("maxLevels", out var lvl) ? lvl.GetInt32() : 3;

            SvnTreeNode tree = await _svnService.BuildSvnHierarchyAsync(svnRootUrl, maxLevels);
            return tree;
        }

        private object? HandleLoadCachedSvnHierarchy(JsonElement payload)
        {
            SvnTreeNode? tree = _svnService.LoadCachedSvnHierarchy();
            return tree;
        }

        private object? HandleBackup(JsonElement payload)
        {
            var rows = GetRows(payload);
            string filePath = GetString(payload, "filePath");
            string path = _exportService.BackupToTsv(rows, filePath);
            return new { filePath = path };
        }

        private object? HandleRestore(JsonElement payload)
        {
            string filePath = GetString(payload, "filePath");
            List<ComparisonRow> rows = _exportService.RestoreFromTsv(filePath);
            return new { rows };
        }

        private object? HandleExportExcel(JsonElement payload)
        {
            var rows = GetRows(payload);
            string message = _exportService.ExportToExcel(rows);
            return new { message };
        }

        private object? HandleExportTsv(JsonElement payload)
        {
            var rows = GetRows(payload);
            string directory = GetString(payload, "directory");
            string fileNamePrefix = GetString(payload, "fileNamePrefix");
            string path = _exportService.ExportToTsv(rows, directory, fileNamePrefix);
            return new { filePath = path };
        }

        private object? HandleExportSummaryTsv(JsonElement payload)
        {
            var rows = GetRows(payload);
            string path = _exportService.ExportSummaryTsv(rows);
            return new { filePath = path };
        }

        private object? HandleExportHtml(JsonElement payload)
        {
            var rows = GetRows(payload);
            string leftText = GetString(payload, "leftText");
            string rightText = GetString(payload, "rightText");
            string html = _exportService.GenerateBranchAnalysisHtml(rows, leftText, rightText);

            string reportsDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, "Reports");
            System.IO.Directory.CreateDirectory(reportsDirectory);
            string filePath = System.IO.Path.Combine(reportsDirectory, $"BranchAnalysis_{DateTime.Now:yyMMddHHmmss}.html");
            System.IO.File.WriteAllText(filePath, html);

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });

            return new { filePath };
        }

        private object? HandleOpenWinMerge(JsonElement payload)
        {
            string winMergeToolPath = GetString(payload, "winMergeToolPath");
            string leftPath = GetString(payload, "leftPath");
            string rightPath = GetString(payload, "rightPath");
            _svnService.OpenWinMerge(winMergeToolPath, leftPath, rightPath);
            return null;
        }

        private object? HandleOpenSvnDiff(JsonElement payload)
        {
            string tortoiseSvnPath = GetString(payload, "tortoiseSvnPath");
            string leftPath = GetString(payload, "leftPath");
            string rightPath = GetString(payload, "rightPath");
            _svnService.OpenSvnDiff(tortoiseSvnPath, leftPath, rightPath);
            return null;
        }

        private object? HandleOpenSvnDiffNotepad(JsonElement payload)
        {
            string leftPath = GetString(payload, "leftPath");
            string rightPath = GetString(payload, "rightPath");
            _svnService.OpenSvnDiffInNotepad(leftPath, rightPath);
            return null;
        }

        private object? HandleShowSvnLog(JsonElement payload)
        {
            string tortoiseSvnPath = GetString(payload, "tortoiseSvnPath");
            string svnUrl = GetString(payload, "svnUrl");
            _svnService.ShowSvnLog(tortoiseSvnPath, svnUrl);
            return null;
        }

        private object? HandlePickOpenFile(JsonElement payload)
        {
            string filter = payload.TryGetProperty("filter", out var f) ? f.GetString() ?? "All files (*.*)|*.*" : "All files (*.*)|*.*";
            using var ofd = new System.Windows.Forms.OpenFileDialog { Filter = filter };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return new { filePath = ofd.FileName };
            }
            return new { filePath = (string?)null };
        }

        private object? HandlePickSaveFile(JsonElement payload)
        {
            string filter = payload.TryGetProperty("filter", out var f) ? f.GetString() ?? "All files (*.*)|*.*" : "All files (*.*)|*.*";
            string? fileName = payload.TryGetProperty("fileName", out var fn) ? fn.GetString() : null;
            using var sfd = new System.Windows.Forms.SaveFileDialog { Filter = filter, FileName = fileName };
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return new { filePath = sfd.FileName };
            }
            return new { filePath = (string?)null };
        }

        private static List<ComparisonRow> GetRows(JsonElement payload)
        {
            var rowsElement = payload.GetProperty("rows");
            return JsonSerializer.Deserialize<List<ComparisonRow>>(rowsElement.GetRawText()) ?? new List<ComparisonRow>();
        }

        private static string GetString(JsonElement payload, string name)
        {
            return payload.TryGetProperty(name, out var el) ? el.GetString() ?? string.Empty : string.Empty;
        }

        private static bool GetBool(JsonElement payload, string name)
        {
            return payload.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.True;
        }
    }

    public class BridgeResponse
    {
        [JsonPropertyName("requestId")]
        public string? RequestId { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
