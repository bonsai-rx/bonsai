using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Design;

[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(int))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(float))]
[assembly: TypeVisualizer(typeof(TimeSeriesVisualizer), Target = typeof(double))]

namespace Bonsai.Design
{
    public class TimeSeriesVisualizer : DialogTypeVisualizer
    {
        const double TimeScaleFactor = 42;
        TimeSeriesControl chart;
        List<DateTime> valuesX;
        List<object> valuesY;

        protected TimeSeriesControl Chart
        {
            get { return chart; }
        }

        protected IList<DateTime> ValuesX
        {
            get { return valuesX; }
        }

        protected IList<object> ValuesY
        {
            get { return valuesY; }
        }

        protected void AddValue(DateTime time, object value)
        {
            var excess = valuesX.Where(x => (time - x).TotalSeconds > chart.Width / TimeScaleFactor).Count();
            if (excess > 0)
            {
                valuesX.RemoveRange(0, excess);
                valuesY.RemoveRange(0, excess);
            }

            valuesX.Add(time);
            valuesY.Add(value);

            chart.TimeSeries.Points.DataBindXY(valuesX, valuesY);
        }

        public override void Show(object value)
        {
            AddValue(DateTime.Now, value);
        }

        public override void Load(IServiceProvider provider)
        {
            chart = new TimeSeriesControl();
            valuesX = new List<DateTime>();
            valuesY = new List<object>();

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(chart);
            }
        }

        public override void Unload()
        {
            chart.Dispose();
            chart = null;
        }
    }
}
