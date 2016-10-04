using KJade.Util;
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

        private int ReadIndentLevel(string line, string indentIndicator)
        {
            if (indentIndicator == "") return 0; //If no indents, then the indent level is 0
            string rl = line;
            string ii = indentIndicator;
            int indLv = 0;
            for (int i = 1; rl.StartsWith(ii, System.StringComparison.CurrentCulture); i++)
            {
                rl.EatString(ii);
                indLv = i;
            }
            return indLv;
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

        private void ReadCode(string input)
        {
            input = input.Strip(IgnoredCharacters); //Strip useless characters
            string[] codeLines = input.Split('\n');
            string indentIndicator = InferIndentIndicator(codeLines);
            foreach (string line in codeLines)
            {
                var indentLevel = ReadIndentLevel(line, indentIndicator);
                var lToken = new RawToken { };
            }
        }
    }
}