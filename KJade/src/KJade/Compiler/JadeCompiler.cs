using System.IO;

namespace KJade.Compiler
{
    public abstract class JadeCompiler : IJadeCompiler
    {
        public virtual IJadeCompileResult Compile(TextReader input)
        {
            return null; //TODO: change this!
        }
    }
}