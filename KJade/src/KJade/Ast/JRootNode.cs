using System.Collections.Generic;

namespace KJade.Ast
{
    /// <summary>
    /// A node representing a root node.
    /// </summary>
    public class JRootNode
    {
        public List<JNode> Children { get; set; } = new List<JNode>();
    }
}