using KJade.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KJade.Parser
{
    public class JadeLexer
    {
        public string[] IgnoredCharacters { get; } = new string[] { "\r" };

        public void ReadCode(StringBuilder input)
        {
            ReadCode(input.ToString());
        }

        private Tuple<int, string> ReadIndentedToken(string line, string indentIndicator)
        {
            if (indentIndicator == "") return new Tuple<int, string>(0, line); //If no indents, then the indent level is 0
            string rl = line;
            string ii = indentIndicator;
            int indLv = 0;
            for (int i = 1; rl.StartsWith(ii, System.StringComparison.CurrentCulture); i++)
            {
                rl = rl.EatString(ii);
                indLv = i;
            }
            return new Tuple<int, string>(indLv, rl);
        }

        private string InferIndentIndicator(string[] codeLines)
        {
            //Read some raw indents
            foreach (string line in codeLines)
            {
                string rl = line;
                //Check if the first character is an indent.
                //If not, just continue in the loop.
                switch (rl[0])
                {
                    case ' ':
                        //Space indents
                        //We have to find out how many spaces
                        int sc = 0; //space count
                        for (int i = 1; rl[0] == ' '; i++)
                        {
                            rl = rl.Substring(1);
                            sc = i;
                        }
                        return new string(' ', sc);

                    case '\t':
                        //Tab indents
                        return "\t";
                }
            }
            //None of the lines seemed indented.
            return "";
        }

        public List<JadeToken> ReadCode(string input)
        {
            input = input.Strip(IgnoredCharacters); //Strip useless characters

            Queue<RawToken> rawTokens = new Queue<RawToken>(); //a queue of raw tokens to be processed
            string[] codeLines = input.Split('\n');
            string indentIndicator = InferIndentIndicator(codeLines);
            //Process indentation structure
            foreach (string line in codeLines)
            {
                //Read an indented token that has an indentation amount and a value not including the indent characters
                var indentedToken = ReadIndentedToken(line, indentIndicator);

                var lToken = new RawToken
                {
                    IndentLevel = indentedToken.Item1,
                    Value = indentedToken.Item2,
                };
                rawTokens.Enqueue(lToken);
            }
            //Create real Token objects based on the indentation structure
            List<JadeToken> jadeTokens = new List<JadeToken>();
            bool multilineScope = false; //whether we are in a multiline scope.
            int multilineScopeStart = 0;
            int previousIndentLevel = 0;
            foreach (var rawTok in rawTokens)
            {
                var processedTokValue = rawTok.Value;
                if (multilineScope) //We're in a special area, normal rules don't apply
                {
                    if (rawTok.IndentLevel < multilineScopeStart)
                    {
                        //The current indent level is less than where the multiline scope started
                        //Exit the multiline scope
                        multilineScope = false;
                        multilineScopeStart = 0;
                    }
                    else
                    {
                        //We're still in the multiline scope. Edit the previous token.
                        //Grab the last token
                        var prevTok = jadeTokens.Last();

                        //Append the current token's value to the previous token, along with a whitespace character
                        prevTok.Value += rawTok.Value + " ";
                        continue;
                    }
                }

                //Normal node token
                //Look for indicators showing end of node name
                var nodeRegex = new Regex(@"^\w+(?=(\.|#|\())", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var nodeNameMatch = nodeRegex.Match(processedTokValue); //Only match one
                //If the match succeeded, there was more than just the node name.
                //Otherwise, the only part of the token value was the node name anyway
                var nodeName = nodeNameMatch.Success ? nodeNameMatch.Value : processedTokValue;
                nodeRegex.Replace(processedTokValue, ""); //Remove matched part of value

                var classes = new List<string>();
                var nodeAttributes = new Dictionary<string, string>();
                var nodeIdAttribute = string.Empty;

                //Attempt to read classes
                var classRegex = new Regex(@"\.(\w|-)+");
                var classMatches = classRegex.Matches(processedTokValue);
                //Add matched class names to classes collection
                foreach (Match classMatch in classMatches)
                {
                    classes.Add(classMatch.Value);
                }
                classRegex.Replace(processedTokValue, ""); //Remove matched part of value

                //Attempt to read id
                var idRegex = new Regex(@"\#(\w|-)+");
                var idMatch = idRegex.Match(processedTokValue);
                nodeIdAttribute = idMatch.Success ? idMatch.Value : nodeIdAttribute;
                idRegex.Replace(processedTokValue, ""); //Remove matched part of value

                //Look for an attribute group - looks like this: (attr="some value")
                var attributeGroupRegex = new Regex(@"\(([^\)]+)\)");
                var attributeGroupMatch = attributeGroupRegex.Match(processedTokValue);
                if (attributeGroupMatch.Success)
                {
                    //An attribute group was found!
                    var attributeGroupText = attributeGroupMatch.Value;
                    //Remove the opening ( and the closing )
                    attributeGroupText = attributeGroupText.Substring(1, attributeGroupText.Length - 2);
                    //Parse the attribute group
                    var nameValuePairs = attributeGroupText.Split(',');

                    foreach (var nvp in nameValuePairs)
                    {
                        var components = nvp.Split('=');
                        var attributeName = components[0];
                        var attributeValue = components[1];
                        //clean up attribute value
                        attributeValue = attributeValue.EatString("\""); //Eat the beginning `"` character
                        attributeValue = attributeValue.Last() == '"' ? attributeValue.Substring(0, attributeValue.Length - 1) : attributeValue; //Strip ending quote
                        //Save the attribute
                        nodeAttributes.Add(attributeName, attributeValue);
                    }
                }

                if (processedTokValue.EndsWith(".", StringComparison.CurrentCulture)) //This token isn't ending yet
                {
                    multilineScope = true;
                    multilineScopeStart = rawTok.IndentLevel; //Where the multiline scope started
                }
                var jTok = new JadeToken(nodeName, classes, nodeIdAttribute, nodeAttributes, rawTok.IndentLevel);
                jadeTokens.Add(jTok);
                previousIndentLevel = rawTok.IndentLevel;
            }
            return null; //TODO: update
        }
    }
}