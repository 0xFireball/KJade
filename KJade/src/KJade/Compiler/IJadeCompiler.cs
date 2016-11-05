namespace KJade.Compiler
{
    public interface IJadeCompiler
    {
        /// <summary>
        /// Compiles the given KJade source
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        IJadeCompileResult Compile(string source);

        /// <summary>
        /// Compiles the given KJade source with a model as the data source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        IJadeCompileResult Compile(string source, object model);

        /// <summary>
        /// Returns preprocessed KJade with model substitutions performed
        /// </summary>
        /// <param name="source"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        string PerformSubstitutions(string source, object model);
    }
}