using Bonsai.Expressions;
using System;
using System.Windows.Forms;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a type visualizer to display an object as a bar graph.
    /// </summary>
    public class BarGraphVisualizer : DialogTypeVisualizer
    {
        GraphControl graph;
        BarGraphBuilder.VisualizerController controller;
        IPointListEdit[] barSeries;
        bool labelBars;
        bool reset;

        static void GetBarGraphAxes(BarBase barBase, GraphControl graph, out Axis indexAxis, out Axis valueAxis)
        {
            switch (barBase)
            {
                case BarBase.X:
                case BarBase.X2:
                    indexAxis = graph.GraphPane.XAxis;
                    valueAxis = graph.GraphPane.YAxis;
                    break;
                case BarBase.Y:
                case BarBase.Y2:
                    indexAxis = graph.GraphPane.YAxis;
                    valueAxis = graph.GraphPane.XAxis;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(barBase));
            }
        }

        internal void AddValues(string index, double[] values)
        {
            EnsureSeries(values.Length);
            if (values.Length > 0)
            {
                var count = barSeries[0].Count;
                var updateLast = count > 0 && index.Equals(barSeries[0][count - 1].Tag);
                if (updateLast && controller.BaseAxis <= BarBase.X2) UpdateLastBaseX();
                else if (updateLast) UpdateLastBaseY();
                else if (controller.BaseAxis <= BarBase.X2) AddBaseX();
                else AddBaseY();

                void UpdateLastBaseX()
                {
                    for (int i = 0; i < barSeries.Length; i++)
                        barSeries[i][count - 1].Y = values[i];
                }

                void UpdateLastBaseY()
                {
                    for (int i = 0; i < barSeries.Length; i++)
                        barSeries[i][count - 1].X = values[i];
                }

                void AddBaseX()
                {
                    for (int i = 0; i < barSeries.Length; i++)
                        barSeries[i].Add(new PointPair(0, values[i], index));
                }

                void AddBaseY()
                {
                    for (int i = 0; i < barSeries.Length; i++)
                        barSeries[i].Add(new PointPair(values[i], 0, index));
                }
            }
        }

        void EnsureSeries(int count)
        {
            if (barSeries == null || barSeries.Length != count || reset)
            {
                reset = false;
                graph.ResetColorCycle();
                graph.GraphPane.CurveList.Clear();
                barSeries = new IPointListEdit[count];
                for (int i = 0; i < barSeries.Length; i++)
                {
                    var color = graph.GetNextColor();
                    var values = controller.Capacity > 0
                        ? (IPointListEdit)new RollingPointPairList(controller.Capacity)
                        : new PointPairList();
                    var barItem = new BarItem(labelBars ? controller.ValueLabels[i] : null, values, color);
                    barItem.Bar.Fill.Type = FillType.Solid;
                    barItem.Bar.Border.IsVisible = false;
                    graph.GraphPane.CurveList.Add(barItem);
                    barSeries[i] = values;
                }
            }
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var barChartBuilder = (BarGraphBuilder)ExpressionBuilder.GetVisualizerElement(context.Source).Builder;
            controller = barChartBuilder.Controller;

            graph = new GraphControl();
            graph.Dock = DockStyle.Fill;
            graph.GraphPane.BarSettings.Base = controller.BaseAxis;
            graph.GraphPane.BarSettings.Type = controller.BarType;
            GetBarGraphAxes(barChartBuilder.BaseAxis, graph, out Axis indexAxis, out Axis valueAxis);
            GraphHelper.FormatOrdinalAxis(indexAxis, typeof(string));
            GraphHelper.SetAxisLabel(indexAxis, controller.IndexLabel);
            indexAxis.Scale.IsReverse = controller.BaseAxis == BarBase.Y;
            indexAxis.MajorTic.IsInside = false;

            var hasLabels = controller.ValueLabels != null;
            var labelAxis = hasLabels && controller.ValueLabels.Length == 1;
            if (labelAxis) GraphHelper.SetAxisLabel(valueAxis, controller.ValueLabels[0]);
            labelBars = hasLabels && !labelAxis;
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
            controller.AddValues(value, this);
            graph.Invalidate();
        }

        /// <inheritdoc/>
        public override void SequenceCompleted()
        {
            reset = true;
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            graph.Dispose();
            graph = null;
            controller = null;
            barSeries = null;
            reset = false;
        }
    }
}
