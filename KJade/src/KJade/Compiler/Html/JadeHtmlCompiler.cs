using KJade.Ast;
using System.Linq;
using System.Text;

namespace KJade.Compiler.Html
{
    public class JadeHtmlCompiler : JadeCompiler
    {
        public JadeHtmlCompilerOptions Options { get; }

        public JadeHtmlCompiler(JadeHtmlCompilerOptions options)
        {
            Options = options;
        }

        private HtmlNode GetHtmlNode(JNode jnode)
        {
            var retNode = new HtmlNode
            {
                Attributes = jnode.Attributes,
                Children = jnode.Children.Select(n => GetHtmlNode(n)).ToList(),
                Classes = jnode.Classes,
                Element = jnode.Element,
                Id = jnode.Id,
                IndentationLevel = jnode.IndentationLevel,
                Value = jnode.Value,
            };
            if (retNode.Element == null) retNode.Element = "div"; //Implicity understand it's a div
            //Change formats of classes and IDs
            if (retNode.Classes != null)
            {
                retNode.Classes = retNode.Classes.Select(c => c.Substring(1)).ToList(); //Remove the starting period
            }
            if (retNode.Id != null)
            {
                retNode.Id = retNode.Id.Substring(1); //Remove the starting #
            }
            return retNode;
        }

        public string BuildAttributeString(HtmlNode node)
        {
            StringBuilder attrStrBuilder = new StringBuilder();

            var idAttr = string.IsNullOrEmpty(node.Id) ? "" : Options.Minify ? $" id = \"{node.Id}\"" : $" id=\"{node.Id}\"";
            var classAttr = node.Classes == null || node.Classes.Count == 0 ? "" : Options.Minify ? $" class = \"{string.Join(" ", node.Classes)}\"" : $" class=\"{string.Join(" ", node.Classes)}\"";

            var genericAttrs = node.Attributes == null || node.Attributes.Count == 0 ? "" : Options.Minify ? $"{string.Join(" ", node.Attributes.Select(attr => { return $" {attr.Key} = \"{attr.Value}\""; }))}" : $"{string.Join(" ", node.Attributes.Select(attr => { return $" {attr.Key}=\"{attr.Value}\""; }))}";

            attrStrBuilder.Append(idAttr);
            attrStrBuilder.Append(classAttr);
            attrStrBuilder.Append(genericAttrs);
            return attrStrBuilder.ToString();
        }

        private void EmitHtml(HtmlNode rootNode, StringBuilder outputBuilder)
        {
            var attributes = BuildAttributeString(rootNode);
            outputBuilder.Append("<" + rootNode.Element + $" {attributes}>");
            if (Options.Minify)
            {
                outputBuilder.AppendLine();
            }
            outputBuilder.Append(rootNode.Value);
            if (Options.Minify)
            {
                outputBuilder.AppendLine();
            }
            //Recursively emit children
            foreach (var nodeChild in rootNode.Children)
            {
                EmitHtml(nodeChild, outputBuilder);
            }
            outputBuilder.Append("</" + rootNode.Element + ">");
            if (Options.Minify)
            {
                outputBuilder.AppendLine();
            }
        }

        protected override IJadeCompileResult CompileFromAst(JRootNode ast)
        {
            var result = new HtmlCompileResult();
            //Compile the KJade AST to a HTML AST
            var htmlAst = GetHtmlNode(ast);
            //Emit on first level nodes because the root is not a real node
            foreach (var firstLevelNode in htmlAst.Children)
            {
                EmitHtml(firstLevelNode, result.Value);
            }
            return result;
        }
    }
}