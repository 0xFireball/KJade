using System.Collections.Generic;
using System.Text;

namespace KJade.Ast
{
    public abstract class JNode
    {
        public List<JNode> Children { get; set; } = new List<JNode>();

        public StringBuilder TextRepresentation { get; set; } = new StringBuilder();
    }
}