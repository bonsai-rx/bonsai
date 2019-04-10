using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using Bonsai;
using Bonsai.Design.Visualizers;

[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<byte>), Target = typeof(Timestamped<byte>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<int>), Target = typeof(Timestamped<int>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<uint>), Target = typeof(Timestamped<uint>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<long>), Target = typeof(Timestamped<long>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<ulong>), Target = typeof(Timestamped<ulong>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<short>), Target = typeof(Timestamped<short>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<ushort>), Target = typeof(Timestamped<ushort>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<float>), Target = typeof(Timestamped<float>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<double>), Target = typeof(Timestamped<double>))]

namespace Bonsai.Design.Visualizers
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
