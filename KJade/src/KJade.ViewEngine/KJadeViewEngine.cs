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

        public KJadeViewEngine(SuperSimpleViewEngine engineWrapper)
        {
            this.engineWrapper = engineWrapper;
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

        private static readonly Regex ImportRegex = new Regex(@"@import\s(?<ViewName>\w+)", RegexOptions.Compiled);

        public Response RenderView(ViewLocationResult viewLocationResult, dynamic model, IRenderContext renderContext)
        {
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
        }

        private string EvaluateKJade(ViewLocationResult viewLocationResult, dynamic model, IRenderContext renderContext)
        {
            string content;
            using (var reader = viewLocationResult.Contents.Invoke())
            {
                content = reader.ReadToEnd();
            }

            //Recursively replace @import
            content = ImportRegex.Replace(content, m =>
            {
                var partialViewName = m.Groups["ViewName"].Value;
                var partialModel = model;
                return EvaluateKJade(renderContext.LocateView(partialViewName, partialModel), model, renderContext);
            });

            var jadeCompiler = new JadeHtmlCompiler();
            var compiledHtml = jadeCompiler.Compile(content, model);
            return compiledHtml.Value.ToString();
        }
    }
}