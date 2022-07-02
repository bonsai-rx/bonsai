using System.Drawing;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    class LineGraph : RollingGraph
    {
        public SymbolType SymbolType { get; set; } = SymbolType.None;

        public float LineWidth { get; set; } = 1;

        internal override CurveItem CreateSeries(string label, IPointListEdit points, Color color)
        {
            var curve = new LineItem(label, points, color, SymbolType, LineWidth);
            curve.Line.IsAntiAlias = true;
            curve.Line.IsOptimizedDraw = true;
            curve.Label.IsVisible = !string.IsNullOrEmpty(label);
            curve.Symbol.Fill.Type = FillType.Solid;
            curve.Symbol.IsAntiAlias = true;
            return curve;
        }
    }
}
