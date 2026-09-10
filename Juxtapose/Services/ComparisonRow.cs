using System;

namespace Juxtapose.Services
{
    public class ComparisonRow
    {
        public string Status { get; set; } = string.Empty;
        public string Left { get; set; } = string.Empty;
        public string Right { get; set; } = string.Empty;
        public string Added { get; set; } = "0";
        public string Deleted { get; set; } = "0";
        public string Modified { get; set; } = "0";
        public string Total { get; set; } = "0";
        public string ChangePercent { get; set; } = "0";
        public string Revisions { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#FFFFFF";
        public bool Visible { get; set; } = true;
    }
}
