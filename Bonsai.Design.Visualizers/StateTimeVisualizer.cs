using System;
using ZedGraph;
using System.Windows.Forms;

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a type visualizer for 
    /// </summary>
    public class StateTimeVisualizer : DialogTypeVisualizer
    {
        int stateIndex;
        const int StateColumns = 10;
        GraphControl graph;
        readonly RollingPointPairList values;

        object state;
        DateTime stateEnter;

        /// <summary>
        /// Initializes a new instance of the <see cref="StateTimeVisualizer"/> class.
        /// </summary>
        public StateTimeVisualizer()
        {
            values = new RollingPointPairList(StateColumns);
        }

        /// <summary>
        /// Gets the underlying graph control.
        /// </summary>
        protected GraphControl Graph
        {
            get { return graph; }
        }

        internal void AddValue(DateTime time, object value)
        {
            if (value == null) return;
            if (values.Count > 0)
            {
                var diff = time - stateEnter;
                values[values.Count - 1].Y = diff.TotalSeconds;
            }

            if (!value.Equals(state))
            {
                state = value;
                stateEnter = time;
                values.Add(stateIndex++, 0, value.ToString());

                var textLabels = new string[values.Count];
                for (int i = 0; i < textLabels.Length; i++)
                {
                    textLabels[i] = (string)values[i].Tag;
                }
                graph.GraphPane.XAxis.Scale.TextLabels = textLabels;
            }

            graph.Invalidate();
        }

        /// <inheritdoc/>
        public override void Show(object value)
        {
            AddValue(DateTime.Now, value);
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            graph = new GraphControl();
            graph.Dock = DockStyle.Fill;
            graph.GraphPane.Chart.Border.IsVisible = true;
            graph.GraphPane.YAxis.MajorGrid.IsVisible = true;
            graph.GraphPane.YAxis.MajorGrid.DashOff = 0;
            graph.GraphPane.YAxis.Title.IsVisible = true;
            graph.GraphPane.YAxis.Title.Text = "Time (s)";
            graph.GraphPane.XAxis.Type = AxisType.Text;
            graph.GraphPane.XAxis.Scale.FontSpec.Angle = 90;
            graph.GraphPane.XAxis.MajorTic.IsInside = false;
            graph.GraphPane.XAxis.MinorTic.IsAllTics = false;
            graph.GraphPane.XAxis.MajorGrid.IsVisible = true;
            graph.GraphPane.XAxis.MajorGrid.DashOff = 0;
            Graph.GraphPane.XAxis.Title.IsVisible = true;
            Graph.GraphPane.XAxis.Title.Text = "State";
            var barSeries = new BarItem(string.Empty, values, graph.GetNextColor());
            barSeries.Bar.Fill.Type = FillType.Solid;
            barSeries.Bar.Border.IsVisible = false;
            graph.GraphPane.CurveList.Add(barSeries);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(graph);
            }
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            values.Clear();
            state = null;

            graph.Dispose();
            graph = null;
        }
    }
}
