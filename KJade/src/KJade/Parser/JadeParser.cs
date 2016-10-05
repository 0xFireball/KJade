using KJade.Ast;
using System.Collections.Generic;

namespace KJade.Parser
{
    /// <summary>
    /// Parses a list of tokens and builds an AST.
    /// </summary>
    public class JadeParser
    {
        public JNode CreateNodeFromJadeToken(JadeToken inputToken)
        {
            return new JNode
            {
                Attributes = inputToken.Attributes,
                Classes = inputToken.Classes,
                Element = inputToken.NodeName,
                Id = inputToken.Id,
                Value = inputToken.Value,
            };
        }

        public JRootNode ParseTokens(List<JadeToken> tokens)
        {
            JRootNode rootNode = new JRootNode();
            //Parse the input tokens and build an AST.
            Stack<int> nestingLevel = new Stack<int>();
            nestingLevel.Push(0); //Push a zero
            foreach (var jToken in tokens)
            {
            }
            nestingLevel.Pop(); //We've reached the end!
            return rootNode;
        }
    }
}