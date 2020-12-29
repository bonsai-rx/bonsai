using Bonsai.Expressions;
using System;
using System.Windows.Forms;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    public class LineGraphVisualizer : DialogTypeVisualizer
    {
        GraphControl graph;
        LineGraphBuilder.VisualizerController controller;
        IPointListEdit[] lineSeries;

        internal void AddValues(PointPair[] values)
        {
            EnsureSeries(values.Length);
            if (values.Length > 0)
            {
                for (int i = 0; i < lineSeries.Length; i++)
                {
                    lineSeries[i].Add(values[i]);
                }
            }
        }

        void EnsureSeries(int count)
        {
            if (lineSeries == null || lineSeries.Length != count)
            {
                graph.ResetColorCycle();
                graph.GraphPane.CurveList.Clear();
                lineSeries = new IPointListEdit[count];
                var labelLines = controller.ValueLabels != null;
                if (labelLines && controller.ValueLabels.Length == 2 && count == 1)
                {
                    labelLines = false;
                    GraphHelper.SetAxisLabel(graph.GraphPane.XAxis, controller.ValueLabels[0]);
                    GraphHelper.SetAxisLabel(graph.GraphPane.YAxis, controller.ValueLabels[1]);
                }

                for (int i = 0; i < lineSeries.Length; i++)
                {
                    var color = graph.GetNextColor();
                    var values = controller.Capacity > 0
                        ? (IPointListEdit)new RollingPointPairList(controller.Capacity)
                        : new PointPairList();
                    var lineItem = new LineItem(
                        labelLines ? controller.ValueLabels[i] : null,
                        values,
                        color,
                        controller.SymbolType,
                        controller.LineWidth);
                    lineItem.Line.IsAntiAlias = true;
                    lineItem.Line.IsOptimizedDraw = true;
                    lineItem.Symbol.Fill.Type = FillType.Solid;
                    lineItem.Symbol.IsAntiAlias = true;
                    graph.GraphPane.CurveList.Add(lineItem);
                    lineSeries[i] = values;
                }
            }
        }

        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var lineChartBuilder = (LineGraphBuilder)ExpressionBuilder.GetVisualizerElement(context.Source).Builder;
            controller = lineChartBuilder.Controller;

            graph = new GraphControl();
            graph.Dock = DockStyle.Fill;

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(graph);
            }
        }

        public override void Show(object value)
        {
            controller.AddValues(value, this);
            graph.Invalidate();
        }

        public override void Unload()
        {
            graph.Dispose();
            graph = null;
            controller = null;
            lineSeries = null;
        }
    }
}
