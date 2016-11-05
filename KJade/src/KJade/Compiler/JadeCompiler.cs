using KJade.Ast;
using KJade.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KJade.Compiler
{
    public abstract class JadeCompiler : IJadeCompiler
    {
        private static readonly Regex SingleVariableSubstitutionRegex = new Regex(@"(?<Encode>!)?#{model(?:\.(?<ParameterName>[a-zA-Z0-9-_]+))}", RegexOptions.Compiled);
        private static readonly Regex ConditionalRegex = new Regex(@"@if(?<Not>not)?(?<AllowNonexistent>\?)?\smodel(?:\.(?<ParameterName>[a-zA-Z0-9-_]+)+)?(?<Contents>[\s\S]*?)@endif", RegexOptions.Compiled);

        public static string XmlEncode(string value)
        {
            return value
              .Replace("<", "&lt;")
              .Replace(">", "&gt;")
              .Replace("\"", "&quot;")
              .Replace("'", "&apos;")
              .Replace("&", "&amp;");
        }

        public static string XmlDecode(string value)
        {
            return value
              .Replace("&lt;", "<")
              .Replace("&gt;", ">")
              .Replace("&quot;", "\"")
              .Replace("&apos;", "'")
              .Replace("&amp;", "&");
        }

        public string PerformStandardSubstitutions(string input, object model)
        {
            var substitutionList = new List<Func<string, object, string>>
            {
                PerformSingleSubstitutions,
                PerformConditionalSubstitutions,
            };
            var replacedInput = input;
            substitutionList.ForEach(subfunc=>replacedInput=subfunc(replacedInput, model));
            return replacedInput;
        }

        private string PerformSingleSubstitutions(string input, object model)
        {
            //Replace variables
            var replacedInput = SingleVariableSubstitutionRegex.Replace(input, m =>
            {
                var properties = ModelReflectionUtil.GetCaptureGroupValues(m, "ParameterName");
                var substitution = ModelReflectionUtil.GetPropertyValueFromParameterCollection(model, properties);
                if (!substitution.Item1)
                {
                    return "[ERR!]";
                }

                if (substitution.Item2 == null)
                {
                    return string.Empty;
                }
                return m.Groups["Encode"].Success ? XmlEncode(substitution.Item2.ToString()) : substitution.Item2.ToString();
            });
            return replacedInput;
        }

        private string PerformConditionalSubstitutions(string input, object model)
        {
            //Process conditionals
            var replacedInput = ConditionalRegex.Replace(input, m =>
            {
                var properties = ModelReflectionUtil.GetCaptureGroupValues(m, "ParameterName");
                var propertyVal = ModelReflectionUtil.GetPropertyValueFromParameterCollection(model, properties);
                var allowNonexistent = m.Groups["AllowNonexistent"].Success;
                var negateResult = m.Groups["Not"].Success;

                if (!propertyVal.Item1 && !allowNonexistent)
                {
                    return "[ERR!]";
                }

                bool evaluateResult = propertyVal.Item2 != null;

                //Check if property result is a BOOLEAN
                if (evaluateResult && propertyVal.Item2 is bool?)
                {
                    var booleanPropertyResult = propertyVal.Item2 as bool?;
                    evaluateResult = (bool)booleanPropertyResult;
                }

                if (negateResult)
                {
                    evaluateResult = !evaluateResult;
                    //We don't want them both true, as that will mean:
                    //When we're looking for false, but allowing nonexistent,
                    //true will be output, but negate will make it false :(
                    if (!negateResult || !allowNonexistent)
                    {
                        evaluateResult = !evaluateResult;
                    }
                }

                var conditionalContent = m.Groups["Contents"].Value;

                if (evaluateResult)
                {
                    return conditionalContent;
                }
                return string.Empty;
            });
            return replacedInput;
        }

        public IJadeCompileResult Compile(string input, object model)
        {
            var replacedInput = PerformStandardSubstitutions(input, model);
            return Compile(replacedInput);
        }

        public IJadeCompileResult Compile(string input)
        {
            var lexer = new JadeLexer();
            lexer = new JadeLexer();
            var tokens = lexer.ReadCode(input);
            var parser = new JadeParser();
            var ast = parser.ParseTokens(tokens);
            return CompileFromAst(ast);
        }

        protected abstract IJadeCompileResult CompileFromAst(JRootNode ast);
    }
}