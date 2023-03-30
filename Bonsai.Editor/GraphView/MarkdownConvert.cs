using System.Drawing;
using Markdig;

namespace Bonsai.Editor.GraphView
{
    class MarkdownConvert
    {
        static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

        public static string ToHtml(Font font, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                var html = Markdown.ToHtml(text, Pipeline);
                return $@"<div style=""font-family: '{font.Name}'; line-height: 1em;"">{html}</div>";
            }

            return string.Empty;
        }
    }
}
