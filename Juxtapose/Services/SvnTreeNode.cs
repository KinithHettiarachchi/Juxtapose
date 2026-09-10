using System.Collections.Generic;

namespace Juxtapose.Services
{
    public class SvnTreeNode
    {
        public string Name { get; set; } = string.Empty;
        public List<SvnTreeNode> Children { get; set; } = new();
    }
}
