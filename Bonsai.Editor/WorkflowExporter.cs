using System.Drawing;
using System.Linq;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.GraphView;
using Bonsai.Expressions;

namespace Bonsai.Editor
{
    static class WorkflowExporter
    {
        public static GraphViewControl CreateGraphView(ExpressionBuilderGraph workflow, Font font, SvgRendererFactory iconRenderer)
        {
            var selectedLayout = workflow.ConnectedComponentLayering().ToList();
            return new GraphViewControl
            {
                Font = font,
                IconRenderer = iconRenderer,
                Nodes = selectedLayout
            };
        }

        public static string ExportSvg(ExpressionBuilderGraph workflow, Font font, SvgRendererFactory iconRenderer)
        {
            using var graphView = CreateGraphView(workflow, font, iconRenderer);
            return ExportSvg(graphView);
        }

        public static string ExportSvg(GraphViewControl graphView)
        {
            var bounds = graphView.GetLayoutSize();
            var graphics = new SvgNet.SvgGdi.SvgGraphics();
            graphView.DrawGraphics(graphics, scaleFont: true);
            var svg = graphics.WriteSVGString();
            var attributes = string.Format(
                "<svg width=\"{0}\" height=\"{1}\" ",
                bounds.Width, bounds.Height);
            svg = svg.Replace("<svg ", attributes);
            return svg;
        }

        public static Bitmap ExportBitmap(ExpressionBuilderGraph workflow, Font font, SvgRendererFactory iconRenderer)
        {
            using var graphView = CreateGraphView(workflow, font, iconRenderer);
            return ExportBitmap(graphView);
        }

        public static Bitmap ExportBitmap(GraphViewControl graphView)
        {
            var bounds = graphView.GetLayoutSize();
            var bitmap = new Bitmap((int)bounds.Width, (int)bounds.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var gdi = new SvgNet.SvgGdi.GdiGraphics(graphics);
                graphView.DrawGraphics(gdi, scaleFont: false);
                return bitmap;
            }
        }
    }
}
