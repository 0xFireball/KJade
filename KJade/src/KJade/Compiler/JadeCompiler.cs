using KJade.Ast;
using KJade.Parser;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KJade.Compiler
{
    public abstract class JadeCompiler : IJadeCompiler
    {
        private static readonly Regex SingleVariableSubstitutionRegex = new Regex(@"(?<Encode>!)?#{model(?:\.(?<ParameterName>[a-zA-Z0-9-_]+))}", RegexOptions.Compiled);

        /// <summary>
        /// Gets an IEnumerable of capture group values
        /// </summary>
        /// <param name="m">The match to use.</param>
        /// <param name="groupName">Group name containing the capture group.</param>
        /// <returns>IEnumerable of capture group values as strings.</returns>
        private static IEnumerable<string> GetCaptureGroupValues(Match m, string groupName)
        {
            return m.Groups[groupName].Captures.Cast<Capture>().Select(c => c.Value);
        }

        /// <summary>
        /// <para>
        /// Gets a property value from the given model.
        /// </para>
        /// <para>
        /// Anonymous types, standard types and ExpandoObject are supported.
        /// Arbitrary dynamics (implementing IDynamicMetaObjectProvicer) are not, unless
        /// they also implement IDictionary string, object for accessing properties.
        /// </para>
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="propertyName">The property name to evaluate.</param>
        /// <returns>Tuple - Item1 being a bool for whether the evaluation was sucessful, Item2 being the value.</returns>
        /// <exception cref="ArgumentException">Model type is not supported.</exception>
        private static Tuple<bool, object> GetPropertyValue(object model, string propertyName)
        {
            if (model == null || string.IsNullOrEmpty(propertyName))
            {
                return new Tuple<bool, object>(false, null);
            }

            if (!typeof(IDynamicMetaObjectProvider).IsAssignableFrom(model.GetType()))
            {
                return StandardTypePropertyEvaluator(model, propertyName);
            }

            if (typeof(IDictionary<string, object>).IsAssignableFrom(model.GetType()))
            {
                return DynamicDictionaryPropertyEvaluator(model, propertyName);
            }

            throw new ArgumentException("model must be a standard type or implement IDictionary<string, object>", "model");
        }

        /// <summary>
        /// Gets a property value from a collection of nested parameter names
        /// </summary>
        /// <param name="model">The model containing properties.</param>
        /// <param name="parameters">A collection of nested parameters (e.g. User, Name</param>
        /// <returns>Tuple - Item1 being a bool for whether the evaluation was sucessful, Item2 being the value.</returns>
        private static Tuple<bool, object> GetPropertyValueFromParameterCollection(object model, IEnumerable<string> parameters)
        {
            if (parameters == null)
            {
                return new Tuple<bool, object>(true, model);
            }

            var currentObject = model;

            foreach (var parameter in parameters)
            {
                var currentResult = GetPropertyValue(currentObject, parameter);

                if (currentResult.Item1 == false)
                {
                    return new Tuple<bool, object>(false, null);
                }

                currentObject = currentResult.Item2;
            }

            return new Tuple<bool, object>(true, currentObject);
        }

        /// <summary>
        /// A property extractor for standard types.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>Tuple - Item1 being a bool for whether the evaluation was sucessful, Item2 being the value.</returns>
        private static Tuple<bool, object> StandardTypePropertyEvaluator(object model, string propertyName)
        {
            var properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            var property =
                properties.Where(p => string.Equals(p.Name, propertyName, StringComparison.CurrentCulture))
                .FirstOrDefault();

            return property == null ? new Tuple<bool, object>(false, null) : new Tuple<bool, object>(true, property.GetValue(model, null));
        }

        /// <summary>
        /// A property extractor designed for ExpandoObject, but also for any
        /// type that implements IDictionary string object for accessing its
        /// properties.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns>Tuple - Item1 being a bool for whether the evaluation was sucessful, Item2 being the value.</returns>
        private static Tuple<bool, object> DynamicDictionaryPropertyEvaluator(object model, string propertyName)
        {
            var dictionaryModel = (IDictionary<string, object>)model;

            object output;
            return !dictionaryModel.TryGetValue(propertyName, out output) ? new Tuple<bool, object>(false, null) : new Tuple<bool, object>(true, output);
        }

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

        public IJadeCompileResult Compile(string input, object model)
        {
            //Replace variables
            var replacedInput = SingleVariableSubstitutionRegex.Replace(input, m =>
            {
                var properties = GetCaptureGroupValues(m, "ParameterName");
                var substitution = GetPropertyValueFromParameterCollection(model, properties);
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