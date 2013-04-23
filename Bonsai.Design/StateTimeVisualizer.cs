using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Design;
using ZedGraph;
using System.Drawing;

namespace Bonsai.Design
{
    public class StateTimeVisualizer : DialogTypeVisualizer
    {
        int stateIndex;
        const int StateColumns = 10;
        ChartControl chart;
        RollingPointPairList values;

        object state;
        DateTime stateEnter;

        public StateTimeVisualizer()
        {
            values = new RollingPointPairList(StateColumns);
        }

        protected ChartControl Chart
        {
            get { return chart; }
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
                chart.GraphPane.XAxis.Scale.TextLabels = textLabels;
            }

            chart.AxisChange();
            chart.Invalidate();
        }

        public override void Show(object value)
        {
            AddValue(DateTime.Now, value);
        }

        public override void Load(IServiceProvider provider)
        {
            chart = new ChartControl();
            chart.GraphPane.Chart.Border.IsVisible = true;
            chart.GraphPane.YAxis.MajorGrid.IsVisible = true;
            chart.GraphPane.YAxis.MajorGrid.DashOff = 0;
            chart.GraphPane.YAxis.Title.IsVisible = true;
            chart.GraphPane.YAxis.Title.Text = "Time (s)";
            chart.GraphPane.XAxis.Type = AxisType.Text;
            chart.GraphPane.XAxis.Scale.FontSpec.Angle = 90;
            chart.GraphPane.XAxis.MajorTic.IsInside = false;
            chart.GraphPane.XAxis.MinorTic.IsAllTics = false;
            chart.GraphPane.XAxis.MajorGrid.IsVisible = true;
            chart.GraphPane.XAxis.MajorGrid.DashOff = 0;
            Chart.GraphPane.XAxis.Title.IsVisible = true;
            Chart.GraphPane.XAxis.Title.Text = "State";
            var barSeries = new BarItem(string.Empty, values, Color.Navy);
            barSeries.Bar.Fill.Brush = new SolidBrush(chart.GetNextColor());
            barSeries.Bar.Border.IsVisible = false;
            chart.GraphPane.CurveList.Add(barSeries);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(chart);
            }
        }

        public override void Unload()
        {
            values.Clear();
            state = null;

            chart.Dispose();
            chart = null;
        }
    }
}
