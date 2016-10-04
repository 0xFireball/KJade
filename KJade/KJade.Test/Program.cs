using KJade.Compiler.Html;
using System.IO;

namespace KJade.Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var compiler = new JadeHtmlCompiler();
            compiler.Compile(File.ReadAllText("test.jade"));
        }
    }
}