using KJade.Util;
using System;
using System.Collections.Generic;
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
            Queue<JadeToken> jadeTokens = new Queue<JadeToken>();
            bool multilineScope = false; //whether we are in a multiline scope.
            int multilineScopeStart = 0;
            int previousIndentLevel = 0;
            foreach (var rawTok in rawTokens)
            {
                var tokValue = rawTok.Value;
                if (multilineScope) //We're in a special area, normal rules don't apply
                {
                    if (rawTok.IndentLevel < multilineScopeStart)
                    {
                        //The current indent level is less than where the multiline scope started
                        //Exit the multiline scope
                        multilineScope = false;
                        multilineScopeStart = 0;
                    }
                }

                //Normal node token
                //Look for indicators showing end of node name
                var nodeRegex = new Regex(@"^\w+(?=(\.|#|\())", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                var nodeNameMatch = nodeRegex.Match(tokValue); //Only match one
                //If the match succeeded, there was more than just the node name.
                //Otherwise, the only part of the token value was the node name anyway
                var nodeName = nodeNameMatch.Success ? nodeNameMatch.Value : tokValue;

                var classes = new List<string>();
                var attributes = new Dictionary<string, string>();
                var nodeIdAttribute = string.Empty;

                if (tokValue.EndsWith(".", StringComparison.CurrentCulture)) //This token isn't ending yet
                {
                    multilineScope = true;
                    multilineScopeStart = rawTok.IndentLevel; //Where the multiline scope started
                }
                var jTok = new JadeToken(nodeName, classes, nodeIdAttribute, attributes, rawTok.IndentLevel);
                previousIndentLevel = rawTok.IndentLevel;
            }
            return null; //TODO: update
        }
    }
}