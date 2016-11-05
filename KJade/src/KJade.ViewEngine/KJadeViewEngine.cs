using KJade.Compiler;
using KJade.Compiler.Html;
using Nancy;
using Nancy.Responses;
using Nancy.ViewEngines;
using Nancy.ViewEngines.SuperSimpleViewEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace KJade.ViewEngine
{
    public class KJadeViewEngine : IViewEngine
    {
        private readonly SuperSimpleViewEngine engineWrapper;
        private IJadeCompiler jadeCompiler;

        public KJadeViewEngine(SuperSimpleViewEngine engineWrapper)
        {
            this.engineWrapper = engineWrapper;
            jadeCompiler = new JadeHtmlCompiler();
        }

        public IEnumerable<string> Extensions => new[]
        {
            "jade",
            "kjade",
            "kade",
        };

        public void Initialize(ViewEngineStartupContext viewEngineStartupContext)
        {
            //Nothing to really do here
        }

        private static readonly Regex ImportRegex = new Regex(@"@import\s(?<ViewName>[\w/.]+)", RegexOptions.Compiled);
        private static readonly Regex PartialRegex = new Regex(@"@partial\s(?<ViewName>[\w/.]+)(?<Separator>;)model(?:\.(?<ParameterName>[a-zA-Z0-9-_]+)+)", RegexOptions.Compiled);

        public Response RenderView(ViewLocationResult viewLocationResult, dynamic model, IRenderContext renderContext)
        {
            /*
            var response = new HtmlResponse();
            var html = renderContext.ViewCache.GetOrAdd(viewLocationResult, result =>
            {
                return EvaluateKJade(viewLocationResult, model, renderContext);
            });

            var renderedHtml = html;

            response.Contents = stream =>
            {
                var writer = new StreamWriter(stream);
                writer.Write(renderedHtml);
                writer.Flush();
            };

            return response;
            */
            return new HtmlResponse(contents: s =>
            {
                var writer = new StreamWriter(s);
                var templateContents = renderContext.ViewCache.GetOrAdd(viewLocationResult, vr =>
                {
                    using (var reader = vr.Contents.Invoke())
                        return reader.ReadToEnd();
                });

                var renderedHtml = EvaluateKJade(viewLocationResult, model, renderContext);
                writer.Write(renderedHtml);
                writer.Flush();
            });
        }

        private string ReadView(ViewLocationResult locationResult)
        {
            string content;
            using (var reader = locationResult.Contents.Invoke())
            {
                content = reader.ReadToEnd();
            }
            return content;
        }

        private string PreprocessKJade(string kjade, object model, IRenderContext renderContext)
        {
            //Recursively replace @import
            kjade = ImportRegex.Replace(kjade, m =>
            {
                var partialViewName = m.Groups["ViewName"].Value;
                return PreprocessKJade(ReadView(renderContext.LocateView(partialViewName, model)), model, renderContext);
            });

            //Recursively replace @partial
            kjade = PartialRegex.Replace(kjade, m =>
            {
                var partialViewName = m.Groups["ViewName"].Value;
                var properties = ModelReflectionUtil.GetCaptureGroupValues(m, "ParameterName");
                var propertyVal = ModelReflectionUtil.GetPropertyValueFromParameterCollection(model, properties);
                var partialModel = propertyVal.Item2;
                return PreprocessKJade(ReadView(renderContext.LocateView(partialViewName, partialModel)), partialModel, renderContext);
            });

            //Run Substitutions
            kjade = jadeCompiler.PerformSubstitutions(kjade, model);

            return kjade;
        }

        private string EvaluateKJade(ViewLocationResult viewLocationResult, dynamic model, IRenderContext renderContext)
        {
            string content = ReadView(viewLocationResult);

            content = PreprocessKJade(content, model, renderContext);

            var compiledHtml = jadeCompiler.Compile(content, model);
            return compiledHtml.Value.ToString();
        }
    }
}