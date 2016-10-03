using System.IO;

namespace KJade.Compiler
{
    public interface IJadeCompiler
    {
        IJadeCompileResult Compile(TextReader input);
    }
}