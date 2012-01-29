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

        public override void Show(object value)
        {
            control.Points.AddXY(DateTime.Now, value);
        }

        public override void Load(IServiceProvider provider)
        {
            control = new TimeSeriesControl();
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
