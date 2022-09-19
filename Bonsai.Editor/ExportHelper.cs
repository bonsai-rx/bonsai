using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.GraphView;
using Bonsai.Expressions;

namespace Bonsai.Editor
{
    static class ExportHelper
    {
        const float ReferenceDpi = 96f;
        const float ReferenceFontSize = 8.25f;
        static readonly Font SvgFont = new Font(Control.DefaultFont.FontFamily, ReferenceFontSize);

        static GraphViewControl CreateGraphView(
            ExpressionBuilderGraph workflow,
            Font font,
            SvgRendererFactory iconRenderer,
            Image graphicsProvider)
        {
            var selectedLayout = workflow.ConnectedComponentLayering().ToList();
            return new GraphViewControl
            {
                GraphicsProvider = graphicsProvider,
                Font = font,
                IconRenderer = iconRenderer,
                Nodes = selectedLayout
            };
        }

        public static string ExportSvg(ExpressionBuilderGraph workflow, SvgRendererFactory iconRenderer)
        {
            using var graphicsProvider = new Bitmap(1, 1);
            graphicsProvider.SetResolution(ReferenceDpi, ReferenceDpi);
            using var graphView = CreateGraphView(workflow, SvgFont, iconRenderer, graphicsProvider);
            return ExportSvg(graphView);
        }

        public static string ExportSvg(GraphViewControl graphView)
        {
            var bounds = graphView.GetLayoutSize();
            var graphics = new SvgNet.SvgGraphics();
            graphView.DrawGraphics(graphics);
            return graphics.WriteSVGString(bounds);
        }

        public static Bitmap ExportBitmap(ExpressionBuilderGraph workflow, Font font, SvgRendererFactory iconRenderer)
        {
            using var graphView = CreateGraphView(workflow, font, iconRenderer, graphicsProvider: null);
            return ExportBitmap(graphView);
        }

        public static Bitmap ExportBitmap(GraphViewControl graphView)
        {
            var bounds = graphView.GetLayoutSize();
            var bitmap = new Bitmap((int)bounds.Width, (int)bounds.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var gdi = new SvgNet.GdiGraphics(graphics);
                graphView.DrawGraphics(gdi);
                return bitmap;
            }
        }
    }
}
