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
        public const string EmbeddedUrl = "path.embedded";

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
                return $@"
<html>
  <head>
    <link rel=""stylesheet"" href=""https://{EmbeddedUrl}/light-theme.css"">
    <link rel=""stylesheet"" href=""https://{EmbeddedUrl}/dark-theme.css"">
    <link rel=""stylesheet"" href=""https://{EmbeddedUrl}/base.css"">
  </head>
  <body>
    <div style=""font-family: '{font.Name}'"">{html}</div>
  </body>
</html>";
            }

            return string.Empty;
        }
    }
}
