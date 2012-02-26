using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Design;

[assembly: TypeVisualizer(typeof(BooleanTimeSeriesVisualizer), Target = typeof(bool))]

namespace Bonsai.Design
{
    public class BooleanTimeSeriesVisualizer : TimeSeriesVisualizer
    {
        object previous;

        public override void Show(object value)
        {
            var time = DateTime.Now;
            if (previous != null)
            {
                AddValue(time, previous);
            }

            AddValue(time, value);
            previous = value;
        }

        public override void Load(IServiceProvider provider)
        {
            base.Load(provider);
            Chart.TimeSeries.BorderWidth = 2;
        }

        public override void Unload()
        {
            previous = null;
            base.Unload();
        }
    }
}
