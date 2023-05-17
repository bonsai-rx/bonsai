using System;
using System.Drawing;
using System.IO;
using Markdig;
using Markdig.Renderers;

namespace Bonsai.Editor.GraphView
{
    class MarkdownConvert
    {
        public const string DefaultUrl = "path.localhost";

        public static string ToHtml(Font font, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                using var writer = new StringWriter();
                var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
                var renderer = new HtmlRenderer(writer);
                renderer.BaseUrl = new Uri($"https://{DefaultUrl}/");
                pipeline.Setup(renderer);

                var document = Markdown.Parse(text, pipeline);
                renderer.Render(document);
                writer.Flush();

                var html = writer.ToString();
                return $@"<div style=""font-family: '{font.Name}'"">{html}</div>";
            }

            return string.Empty;
        }
    }
}
