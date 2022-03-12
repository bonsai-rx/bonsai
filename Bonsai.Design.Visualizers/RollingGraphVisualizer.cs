using Bonsai.Expressions;
using System;
using System.Windows.Forms;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a type visualizer to display an object as a rolling graph.
    /// </summary>
    public class RollingGraphVisualizer : DialogTypeVisualizer
    {
        static readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30);
        DateTimeOffset updateTime;
        GraphControl graph;
        RollingGraphBuilder.VisualizerController controller;
        IPointListEdit[] lineSeries;
        bool labelLines;

        internal void AddValues(string index, params double[] values) => AddValues(0, index, values);

        internal void AddValues(double index, params double[] values) => AddValues(index, null, values);

        internal void AddValues(double index, string tag, params double[] values)
        {
            EnsureSeries(values.Length);
            for (int i = 0; i < lineSeries.Length; i++)
            {
                lineSeries[i].Add(new PointPair(index, values[i], tag));
            }
        }

        void EnsureSeries(int count)
        {
            if (lineSeries == null || lineSeries.Length != count)
            {
                graph.ResetColorCycle();
                graph.GraphPane.CurveList.Clear();
                lineSeries = new IPointListEdit[count];
                for (int i = 0; i < lineSeries.Length; i++)
                {
                    var color = graph.GetNextColor();
                    var values = controller.Capacity > 0
                        ? (IPointListEdit)new RollingPointPairList(controller.Capacity)
                        : new PointPairList();
                    var lineItem = new LineItem(labelLines ? controller.ValueLabels[i] : null, values, color, SymbolType.None, 1);
                    lineItem.Line.IsAntiAlias = true;
                    lineItem.Line.IsOptimizedDraw = true;
                    graph.GraphPane.CurveList.Add(lineItem);
                    lineSeries[i] = values;
                }
            }
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var lineChartBuilder = (RollingGraphBuilder)ExpressionBuilder.GetVisualizerElement(context.Source).Builder;
            controller = lineChartBuilder.Controller;

            graph = new GraphControl();
            graph.Dock = DockStyle.Fill;
            var indexAxis = graph.GraphPane.XAxis;
            var valueAxis = graph.GraphPane.YAxis;
            GraphHelper.FormatOrdinalAxis(indexAxis, controller.IndexType);
            GraphHelper.SetAxisLabel(indexAxis, controller.IndexLabel);

            var hasLabels = controller.ValueLabels != null;
            var labelAxis = hasLabels && controller.ValueLabels.Length == 1;
            if (labelAxis) GraphHelper.SetAxisLabel(valueAxis, controller.ValueLabels[0]);
            labelLines = hasLabels && !labelAxis;
            EnsureSeries(hasLabels ? controller.ValueLabels.Length : 0);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(graph);
            }
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var time = DateTime.Now;
            controller.AddValues(value, this);
            if ((time - updateTime) > TargetElapsedTime)
            {
                graph.Invalidate();
                updateTime = time;
            }
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            graph.Dispose();
            graph = null;
            controller = null;
            lineSeries = null;
        }
    }
}
