using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using ZedGraph;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Design.Visualizers
{
    public class StateTimeVisualizer : DialogTypeVisualizer
    {
        int stateIndex;
        const int StateColumns = 10;
        GraphControl graph;
        RollingPointPairList values;

        object state;
        DateTime stateEnter;

        public StateTimeVisualizer()
        {
            values = new RollingPointPairList(StateColumns);
        }

        protected GraphControl Graph
        {
            get { return graph; }
        }

        protected void AddValue(DateTime time, object value)
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

        public override void Show(object value)
        {
            AddValue(DateTime.Now, value);
        }

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
            var barSeries = new BarItem(string.Empty, values, Color.Navy);
            barSeries.Bar.Fill.Brush = new SolidBrush(graph.GetNextColor());
            barSeries.Bar.Border.IsVisible = false;
            graph.GraphPane.CurveList.Add(barSeries);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(graph);
            }
        }

        public override void Unload()
        {
            values.Clear();
            state = null;

            graph.Dispose();
            graph = null;
        }
    }
}
