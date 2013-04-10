using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Design;
using System.Windows.Forms.DataVisualization.Charting;

[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(int))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(float))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(double))]

namespace Bonsai.Design
{
    public class TimeSeriesVisualizer : DialogTypeVisualizer
    {
        const double TimeWindow = 7.5;
        static readonly TimeSpan TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 30);
        TimeSeriesControl chart;
        List<DateTime> valuesX;
        List<object>[] valuesY;
        DateTimeOffset updateTime;

        public TimeSeriesVisualizer()
            : this(1)
        {
        }

        public TimeSeriesVisualizer(int numSeries)
        {
            valuesX = new List<DateTime>();
            valuesY = new List<object>[numSeries];
            for (int i = 0; i < valuesY.Length; i++)
            {
                valuesY[i] = new List<object>();
            }
        }

        protected TimeSeriesControl Chart
        {
            get { return chart; }
        }

        protected void AddValue(DateTime time, params object[] value)
        {
            var excess = valuesX.Where(x => (time - x).TotalSeconds > TimeWindow).Count();
            if (excess > 0)
            {
                valuesX.RemoveRange(0, excess);
                Array.ForEach(valuesY, y => y.RemoveRange(0, excess));
            }

            valuesX.Add(time);
            for (int i = 0; i < valuesY.Length; i++)
            {
                valuesY[i].Add(value[i]);
            }

            if ((time - updateTime) > TargetElapsedTime)
            {
                DataBindValues();
                updateTime = time;
            }
        }

        protected void DataBindValues()
        {
            for (int i = 0; i < valuesY.Length; i++)
            {
                chart.TimeSeries[i].Points.DataBindXY(valuesX, valuesY[i]);
            }
        }

        public override void Show(object value)
        {
            AddValue(DateTime.Now, value);
        }

        public override void Load(IServiceProvider provider)
        {
            chart = new TimeSeriesControl();
            for (int i = 1; i < valuesY.Length; i++)
            {
                var series = chart.TimeSeries.Add(chart.TimeSeries[0].Name + i);
                series.ChartType = chart.TimeSeries[0].ChartType;
                series.XValueType = chart.TimeSeries[0].XValueType;
                series.ChartArea = chart.TimeSeries[0].ChartArea;
            }

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(chart);
            }
        }

        public override void Unload()
        {
            valuesX.Clear();
            Array.ForEach(valuesY, y => y.Clear());

            chart.Dispose();
            chart = null;
        }
    }
}
