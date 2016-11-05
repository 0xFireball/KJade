using KJade.Ast;
using KJade.Parser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KJade.Compiler
{
    public abstract class JadeCompiler : IJadeCompiler
    {
        private static readonly string SubstitutionErrorString = "[ERR!]";
        private static readonly Regex SingleVariableSubstitutionRegex = new Regex(@"(?<Encode>!)?#{model(?:\.(?<ParameterName>[a-zA-Z0-9-_]+))}", RegexOptions.Compiled);
        private static readonly Regex ConditionalRegex = new Regex(@"@if(?<Not>not)?(?<AllowNonexistent>\?)?\smodel(?:\.(?<ParameterName>[a-zA-Z0-9-_]+)+)?(?<Contents>[\s\S]*?)@endif", RegexOptions.Compiled);
        private static readonly Regex EnumerableExpansionRegex = new Regex(@"@enumerable\smodel(?:\.(?<ParameterName>[a-zA-Z0-9-_]+)+)?(?<Contents>[\s\S]*?)@endenumerable", RegexOptions.Compiled);

        private static string XmlEncode(string value)
        {
            return value
              .Replace("<", "&lt;")
              .Replace(">", "&gt;")
              .Replace("\"", "&quot;")
              .Replace("'", "&apos;")
              .Replace("&", "&amp;");
        }

        private static string XmlDecode(string value)
        {
            return value
              .Replace("&lt;", "<")
              .Replace("&gt;", ">")
              .Replace("&quot;", "\"")
              .Replace("&apos;", "'")
              .Replace("&amp;", "&");
        }

        private string PerformStandardSubstitutions(string input, object model)
        {
            var substitutionList = new List<Func<string, object, string>>
            {
                PerformSingleSubstitutions,
                PerformConditionalSubstitutions,
                PerformEnumerableExpansionReplacement
            };
            var replacedInput = input;
            substitutionList.ForEach(subfunc => replacedInput = subfunc(replacedInput, model));
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
                    return SubstitutionErrorString;
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

                bool propertyExists = propertyVal.Item1;

                //If the property doesnt exist and nonexistent is not allowed, error
                if (!propertyExists && !allowNonexistent)
                {
                    return SubstitutionErrorString;
                }

                //Check if the property isn't null
                bool evaluateResult = propertyVal.Item2 != null;

                //Check if property result is a BOOLEAN
                if (evaluateResult && propertyVal.Item2 is bool?) //if the property isn't null, and if it's a boolean
                {
                    //Get the actual boolean value
                    var booleanPropertyResult = propertyVal.Item2 as bool?;
                    evaluateResult = (bool)booleanPropertyResult;
                }

                if (negateResult) //A NOT is present
                {
                    evaluateResult = !evaluateResult; //negate
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

        private string PerformEnumerableExpansionReplacement(string input, object model)
        {
            //Replace variables
            var replacedInput = EnumerableExpansionRegex.Replace(input, m =>
            {
                var properties = ModelReflectionUtil.GetCaptureGroupValues(m, "ParameterName");
                var propertyVal = ModelReflectionUtil.GetPropertyValueFromParameterCollection(model, properties);

                if (!propertyVal.Item1) //Enumerable doesn't exist
                {
                    return string.Empty;
                }

                var enumerable = propertyVal.Item2 as IEnumerable;
                if (enumerable == null) //The object wasn't an enumerable
                {
                    return SubstitutionErrorString;
                }

                var expandedEnumerableBody = new StringBuilder();
                var enumerableContent = m.Groups["Contents"].Value;
                var enumerableItemRegex = new Regex(@"{\$element}", RegexOptions.Compiled);
                foreach (var item in enumerable)
                {
                    var replacedContent = enumerableItemRegex.Replace(enumerableContent, item.ToString());
                    expandedEnumerableBody.AppendLine(replacedContent);
                }

                var expandedEnumerableStr = expandedEnumerableBody.ToString();

                return expandedEnumerableStr;
            });
            return replacedInput;
        }

        public string PerformSubstitutions(string source, object model)
        {
            return PerformStandardSubstitutions(source, model);
        }

        public IJadeCompileResult Compile(string source, object model)
        {
            var replacedInput = PerformSubstitutions(source, model);
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