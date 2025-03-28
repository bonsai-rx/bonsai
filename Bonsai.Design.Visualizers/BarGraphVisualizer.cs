using Bonsai.Expressions;
using System;
using System.Windows.Forms;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a type visualizer to display an object as a bar graph.
    /// </summary>
    [Obsolete(ObsoleteMessages.TypeTransferredToGuiPackage)]
    public class BarGraphVisualizer : BufferedVisualizer
    {
        BarGraphBuilder.VisualizerController controller;
        BarGraphView view;
        bool labelBars;
        bool reset;

        /// <summary>
        /// Gets or sets the maximum number of data points displayed at any one moment
        /// in the bar graph.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the lower limit of the y-axis range when using a fixed scale.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// Gets or sets the upper limit of the y-axis range when using a fixed scale.
        /// </summary>
        public double Max { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether the y-axis range should be recalculated
        /// automatically as the graph updates.
        /// </summary>
        public bool AutoScale { get; set; } = true;

        internal void AddValues(string index, params double[] values)
        {
            if (view.Graph.NumSeries != values.Length || reset)
            {
                view.Graph.EnsureCapacity(
                    values.Length,
                    labelBars ? controller.ValueLabels : null,
                    reset);
                reset = false;
            }
            view.Graph.AddValues(index, values);
        }

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

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var barChartBuilder = (BarGraphBuilder)ExpressionBuilder.GetVisualizerElement(context.Source).Builder;
            controller = barChartBuilder.Controller;

            view = new BarGraphView();
            view.Dock = DockStyle.Fill;
            view.Graph.BaseAxis = controller.BaseAxis;
            view.Graph.BarType = controller.BarType;
            GetBarGraphAxes(controller.BaseAxis, view.Graph, out Axis indexAxis, out Axis valueAxis);
            GraphHelper.FormatOrdinalAxis(indexAxis, typeof(string));
            GraphHelper.SetAxisLabel(indexAxis, controller.IndexLabel);
            indexAxis.Scale.IsReverse = controller.BaseAxis == BarBase.Y;
            indexAxis.MajorTic.IsInside = false;

            if (controller.Min.HasValue || controller.Max.HasValue)
            {
                view.AutoScale = false;
                view.AutoScaleVisible = false;
                view.Min = controller.Min.GetValueOrDefault();
                view.Max = controller.Max.GetValueOrDefault();
            }
            else
            {
                view.AutoScale = AutoScale;
                if (!AutoScale)
                {
                    view.Min = Min;
                    view.Max = Max;
                }
            }

            if (controller.Capacity.HasValue)
            {
                view.Capacity = controller.Capacity.Value;
                view.CanEditCapacity = false;
            }
            else
            {
                view.Capacity = Capacity;
                view.CanEditCapacity = true;
            }

            var hasLabels = controller.ValueLabels != null;
            var labelAxis = hasLabels && controller.ValueLabels.Length == 1;
            if (labelAxis) GraphHelper.SetAxisLabel(valueAxis, controller.ValueLabels[0]);
            labelBars = hasLabels && !labelAxis;
            if (hasLabels)
            {
                view.Graph.EnsureCapacity(
                    controller.ValueLabels.Length,
                    labelBars ? controller.ValueLabels : null);
            }

            view.HandleDestroyed += delegate
            {
                Min = view.Min;
                Max = view.Max;
                AutoScale = view.AutoScale;
                Capacity = view.Capacity;
            };

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(view);
            }
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            controller.AddValues(value, this);
            view.Graph.Invalidate();
        }

        /// <inheritdoc/>
        public override void SequenceCompleted()
        {
            reset = true;
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            view.Dispose();
            view = null;
            controller = null;
            reset = false;
        }
    }
}
