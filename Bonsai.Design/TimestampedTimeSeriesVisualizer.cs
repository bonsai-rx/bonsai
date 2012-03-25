using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using Bonsai;
using Bonsai.Design;

[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<int>), Target = typeof(Timestamped<int>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<float>), Target = typeof(Timestamped<float>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<double>), Target = typeof(Timestamped<double>))]

namespace Bonsai.Design
{
    public class TimestampedTimeSeriesVisualizer<T> : TimeSeriesVisualizer
    {
        public override void Show(object value)
        {
            var timestamped = (Timestamped<T>)value;
            AddValue(timestamped.Timestamp.DateTime, timestamped.Value);
        }
    }
}
