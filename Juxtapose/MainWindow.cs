using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Juxtapose.Bridge;
using Juxtapose.Services;

namespace Juxtapose
{
    public partial class MainWindow : Form
    {
        private readonly WebView2 _webView;
        private WebViewBridge? _bridge;

        public MainWindow()
        {
            InitializeComponent();

            _webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(_webView);

            Load += MainWindow_Load;
        }

        private async void MainWindow_Load(object? sender, EventArgs e)
        {
            await _webView.EnsureCoreWebView2Async(null);

            ILogService logService = new LogService();
            ISvnService svnService = new SvnService(logService);
            IFileComparisonService fileComparisonService = new FileComparisonService(logService, svnService);
            IExportService exportService = new ExportService();

            _bridge = new WebViewBridge(
                logService,
                svnService,
                fileComparisonService,
                exportService,
                PostMessageToWeb);

            _webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            string indexPath = System.IO.Path.Combine(AppContext.BaseDirectory, "wwwroot", "index.html");
            _webView.CoreWebView2.Navigate(new Uri(indexPath).AbsoluteUri);
        }

        private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            if (_bridge == null)
            {
                return;
            }

            string json = e.WebMessageAsJson;
            string response = await _bridge.HandleMessageAsync(UnwrapJsonString(json));
            PostMessageToWeb(response);
        }

        private static string UnwrapJsonString(string webMessageAsJson)
        {
            // WebMessageAsJson wraps string payloads in quotes when TryGetWebMessageAsString isn't used.
            // Using WebMessageAsJson directly gives the raw JSON the page sent via postMessage(JSON.stringify(...)),
            // so if the page sent a JSON string literal, unwrap it.
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(webMessageAsJson);
                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    return doc.RootElement.GetString() ?? webMessageAsJson;
                }
                return webMessageAsJson;
            }
            catch
            {
                return webMessageAsJson;
            }
        }

        private void PostMessageToWeb(string json)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => PostMessageToWeb(json)));
                return;
            }

            if (_webView.CoreWebView2 == null)
            {
                return;
            }

            _webView.CoreWebView2.PostWebMessageAsJson(json);
        }
    }
}
