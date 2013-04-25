using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Design;
using ZedGraph;

[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(int))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(float))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(double))]

namespace Bonsai.Design
{
    public class TimeSeriesVisualizer : DialogTypeVisualizer
    {
        const int DefaultBufferSize = 640;
        static readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30);

        ChartControl chart;
        RollingPointPairList[] values;
        DateTimeOffset updateTime;

        public TimeSeriesVisualizer()
            : this(1)
        {
        }

        public TimeSeriesVisualizer(int numSeries)
        {
            values = new RollingPointPairList[numSeries];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = new RollingPointPairList(DefaultBufferSize);
            }
        }

        protected ChartControl Chart
        {
            get { return chart; }
        }

        protected void AddValue(DateTime time, params object[] value)
        {
            var ordinalTime = new XDate(time);
            for (int i = 0; i < values.Length; i++)
            {
                values[i].Add(ordinalTime, Convert.ToDouble(value[i]));
            }

            if ((time - updateTime) > TargetElapsedTime)
            {
                chart.AxisChange();
                chart.Invalidate();
                updateTime = time;
            }
        }

        public override void Show(object value)
        {
            AddValue(DateTime.Now, value);
        }

        public override void Load(IServiceProvider provider)
        {
            chart = new ChartControl();
            chart.GraphPane.XAxis.Type = AxisType.DateAsOrdinal;
            chart.GraphPane.XAxis.Title.Text = "Time";
            chart.GraphPane.XAxis.Title.IsVisible = true;
            chart.GraphPane.XAxis.Scale.Format = "HH:mm:ss";
            chart.GraphPane.XAxis.Scale.MajorUnit = DateUnit.Second;
            chart.GraphPane.XAxis.Scale.MinorUnit = DateUnit.Millisecond;
            chart.GraphPane.XAxis.MinorTic.IsAllTics = false;

            for (int i = 0; i < values.Length; i++)
            {
                var series = new LineItem(string.Empty, values[i], chart.GetNextColor(), SymbolType.None);
                series.Line.IsAntiAlias = true;
                series.Line.IsOptimizedDraw = true;
                series.Label.IsVisible = false;
                chart.GraphPane.CurveList.Add(series);
            }

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(chart);
            }
        }

        public override void Unload()
        {
            Array.ForEach(values, y => y.Clear());
            chart.Dispose();
            chart = null;
        }
    }
}
