using System;
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
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<byte[]>), Target = typeof(Timestamped<byte[]>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<int[]>), Target = typeof(Timestamped<int[]>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<uint[]>), Target = typeof(Timestamped<uint[]>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<long[]>), Target = typeof(Timestamped<long[]>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<ulong[]>), Target = typeof(Timestamped<ulong[]>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<short[]>), Target = typeof(Timestamped<short[]>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<ushort[]>), Target = typeof(Timestamped<ushort[]>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<float[]>), Target = typeof(Timestamped<float[]>))]
[assembly: TypeVisualizer(typeof(TimestampedTimeSeriesVisualizer<double[]>), Target = typeof(Timestamped<double[]>))]

namespace Bonsai.Design.Visualizers
{
    /// <summary>
    /// Provides a type visualizer for multi-dimensional timestamped data.
    /// </summary>
    /// <typeparam name="T">The type of the elements to visualize.</typeparam>
    public class TimestampedTimeSeriesVisualizer<T> : TimeSeriesVisualizer
    {
        /// <inheritdoc/>
        public override void Show(object value)
        {
            var timestamped = (Timestamped<T>)value;
            if (timestamped.Value is Array array)
            {
                AddValue(timestamped.Timestamp.DateTime, array);
            }
            else AddValue(timestamped.Timestamp.DateTime, Convert.ToDouble(timestamped.Value));
        }
    }
}
