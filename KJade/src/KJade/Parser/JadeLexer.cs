using KJade.Util;
using System;
using System.Collections.Generic;
using System.Text;

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
            foreach (var rawTk in rawTokens)
            {
                
            }
            return null; //TODO: update
        }
    }
}