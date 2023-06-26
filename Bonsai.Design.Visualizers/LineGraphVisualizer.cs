using Bonsai.Expressions;
using System;
using System.Windows.Forms;
using ZedGraph;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a type visualizer to display an object as a line graph.
    /// </summary>
    public class LineGraphVisualizer : BufferedVisualizer
    {
        LineGraphBuilder.VisualizerController controller;
        LineGraphView view;
        bool labelLines;
        bool reset;

        /// <summary>
        /// Gets or sets the maximum number of points displayed at any one moment in the graph.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Gets or sets the lower limit of the x-axis range when using a fixed scale.
        /// </summary>
        public double XMin { get; set; }

        /// <summary>
        /// Gets or sets the upper limit of the x-axis range when using a fixed scale.
        /// </summary>
        public double XMax { get; set; } = 1;

        /// <summary>
        /// Gets or sets the lower limit of the y-axis range when using a fixed scale.
        /// </summary>
        public double YMin { get; set; }

        /// <summary>
        /// Gets or sets the upper limit of the y-axis range when using a fixed scale.
        /// </summary>
        public double YMax { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether the x-axis range should be recalculated
        /// automatically as the graph updates.
        /// </summary>
        public bool AutoScaleX { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the y-axis range should be recalculated
        /// automatically as the graph updates.
        /// </summary>
        public bool AutoScaleY { get; set; } = true;

        internal void AddValues(PointPair[] values)
        {
            if (view.Graph.NumSeries != values.Length || reset)
            {
                view.Graph.EnsureCapacity(
                    values.Length,
                    labelLines ? controller.ValueLabels : null,
                    reset);
                reset = false;
            }
            view.Graph.AddValues(values);
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            var context = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            var lineChartBuilder = (LineGraphBuilder)ExpressionBuilder.GetVisualizerElement(context.Source).Builder;
            controller = lineChartBuilder.Controller;

            view = new LineGraphView();
            view.Dock = DockStyle.Fill;
            view.Graph.LineWidth = controller.LineWidth;
            view.Graph.SymbolType = controller.SymbolType;
            labelLines = controller.ValueLabels != null;
            if (labelLines && controller.LabelAxes && controller.ValueLabels.Length == 2)
            {
                labelLines = false;
                GraphHelper.SetAxisLabel(view.Graph.GraphPane.XAxis, controller.ValueLabels[0]);
                GraphHelper.SetAxisLabel(view.Graph.GraphPane.YAxis, controller.ValueLabels[1]);
            }

            if (labelLines)
            {
                view.Graph.EnsureCapacity(
                    controller.ValueLabels.Length,
                    labelLines ? controller.ValueLabels : null);
            }

            if (controller.XMin.HasValue || controller.XMax.HasValue)
            {
                view.AutoScaleX = false;
                view.AutoScaleXVisible = false;
                view.XMin = controller.XMin.GetValueOrDefault();
                view.XMax = controller.XMax.GetValueOrDefault();
            }
            else
            {
                view.AutoScaleX = AutoScaleX;
                if (!AutoScaleX)
                {
                    view.XMin = XMin;
                    view.XMax = XMax;
                }
            }

            if (controller.YMin.HasValue || controller.YMax.HasValue)
            {
                view.AutoScaleY = false;
                view.AutoScaleYVisible = false;
                view.YMin = controller.YMin.GetValueOrDefault();
                view.YMax = controller.YMax.GetValueOrDefault();
            }
            else
            {
                view.AutoScaleY = AutoScaleY;
                if (!AutoScaleY)
                {
                    view.YMin = YMin;
                    view.YMax = YMax;
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

            view.HandleDestroyed += delegate
            {
                XMin = view.XMin;
                XMax = view.XMax;
                YMin = view.YMin;
                YMax = view.YMax;
                AutoScaleX = view.AutoScaleX;
                AutoScaleY = view.AutoScaleY;
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
