using KJade.Ast;
using KJade.Parser;
using System.IO;

namespace KJade.Compiler
{
    public abstract class JadeCompiler : IJadeCompiler
    {
        public IJadeCompileResult Compile(TextReader input)
        {
            var lexer = new JadeLexer();
            lexer = new JadeLexer();
            var tokens = lexer.ReadCode(input.ReadToEnd());
            var parser = new JadeParser();
            var ast = parser.ParseTokens(tokens);
            return CompileFromAst(ast);
        }

        public abstract IJadeCompileResult CompileFromAst(JRootNode ast);
    }
}