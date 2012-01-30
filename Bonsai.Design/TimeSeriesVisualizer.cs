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
        TimeSeriesControl control;
        List<DateTime> valuesX;
        List<object> valuesY;

        public override void Show(object value)
        {
            var excess = valuesX.Count - control.Width;
            if (excess > 0)
            {
                valuesX.RemoveRange(0, excess);
                valuesY.RemoveRange(0, excess);
            }

            valuesX.Add(DateTime.Now);
            valuesY.Add(value);

            control.Points.DataBindXY(valuesX, valuesY);
        }

        public override void Load(IServiceProvider provider)
        {
            control = new TimeSeriesControl();
            valuesX = new List<DateTime>();
            valuesY = new List<object>();

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(control);
            }
        }

        public override void Unload()
        {
            control.Dispose();
            control = null;
        }
    }
}
