using Bonsai.Expressions;
using System;
using System.Windows.Forms;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a type visualizer to display an object as a rolling graph.
    /// </summary>
    public class RollingGraphVisualizer : BufferedVisualizer
    {
        static readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30);
        RollingGraphBuilder.VisualizerController controller;
        DateTimeOffset updateTime;
        RollingGraphView view;
        bool labelLines;
        bool reset;

        /// <summary>
        /// Gets or sets the maximum number of time points displayed at any one moment in the graph.
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

        internal void AddValues(string index, params double[] values) => AddValues(0, index, values);

        internal void AddValues(double index, params double[] values) => AddValues(index, null, values);

        internal void AddValues(double index, string tag, params double[] values)
        {
            if (view.Graph.NumSeries != values.Length || reset)
            {
                view.Graph.EnsureCapacity(
                    values.Length,
                    labelLines ? controller.ValueLabels : null,
                    reset);
                reset = false;
            }
            view.Graph.AddValues(index, tag, values);
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var lineChartBuilder = (RollingGraphBuilder)ExpressionBuilder.GetVisualizerElement(context.Source).Builder;
            controller = lineChartBuilder.Controller;

            view = new RollingGraphView();
            view.Dock = DockStyle.Fill;
            view.Graph.SymbolType = controller.SymbolType;
            view.Graph.LineWidth = controller.LineWidth;
            var indexAxis = view.Graph.GraphPane.XAxis;
            var valueAxis = view.Graph.GraphPane.YAxis;
            GraphHelper.FormatOrdinalAxis(indexAxis, controller.IndexType);
            GraphHelper.SetAxisLabel(indexAxis, controller.IndexLabel);
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
            labelLines = hasLabels && !labelAxis;
            if (hasLabels)
            {
                view.Graph.EnsureCapacity(
                    controller.ValueLabels.Length,
                    labelLines ? controller.ValueLabels : null);
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
            Show(DateTime.Now, value);
        }

        /// <inheritdoc/>
        protected override void Show(DateTime time, object value)
        {
            controller.AddValues(time, value, this);
            if ((time - updateTime) > TargetElapsedTime)
            {
                view.Graph.Invalidate();
                updateTime = time;
            }
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
