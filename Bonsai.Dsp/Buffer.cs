using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Projects each element of the sequence into a buffered array based on element count information.")]
    public class Buffer
    {
        [Description("The number of elements in each buffer.")]
        public int Count { get; set; }

        [Description("The optional number of elements to skip between the creation of each buffer.")]
        public int? Skip { get; set; }

        #region One Channel

        public IObservable<Mat> Process(IObservable<byte> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<short> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<ushort> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<int> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<float> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<double> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        #endregion

        #region Two Channels

        TValue[,] ToArray<TValue>(IList<Tuple<TValue, TValue>> source)
        {
            var output = new TValue[2, source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                var tuple = source[i];
                output[0, i] = tuple.Item1;
                output[1, i] = tuple.Item2;
            }
            return output;
        }

        public IObservable<Mat> Process(IObservable<Tuple<byte, byte>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        #endregion

        #region Three Channels

        TValue[,] ToArray<TValue>(IList<Tuple<TValue, TValue, TValue>> source)
        {
            var output = new TValue[3, source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                var tuple = source[i];
                output[0, i] = tuple.Item1;
                output[1, i] = tuple.Item2;
                output[2, i] = tuple.Item3;
            }
            return output;
        }

        public IObservable<Mat> Process(IObservable<Tuple<byte, byte, byte>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short, short>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort, ushort>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int, int>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float, float>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double, double>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        #endregion

        #region Four Channels

        TValue[,] ToArray<TValue>(IList<Tuple<TValue, TValue, TValue, TValue>> source)
        {
            var output = new TValue[4, source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                var tuple = source[i];
                output[0, i] = tuple.Item1;
                output[1, i] = tuple.Item2;
                output[2, i] = tuple.Item3;
                output[3, i] = tuple.Item4;
            }
            return output;
        }

        public IObservable<Mat> Process(IObservable<Tuple<byte, byte, byte, byte>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short, short, short>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort, ushort, ushort>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int, int, int>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float, float, float>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double, double, double>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        #endregion

        #region Five Channels

        TValue[,] ToArray<TValue>(IList<Tuple<TValue, TValue, TValue, TValue, TValue>> source)
        {
            var output = new TValue[5, source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                var tuple = source[i];
                output[0, i] = tuple.Item1;
                output[1, i] = tuple.Item2;
                output[2, i] = tuple.Item3;
                output[3, i] = tuple.Item4;
                output[4, i] = tuple.Item5;
            }
            return output;
        }

        public IObservable<Mat> Process(IObservable<Tuple<byte, byte, byte, byte, byte>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short, short, short, short>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort, ushort, ushort, ushort>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int, int, int, int>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float, float, float, float>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double, double, double, double>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        #endregion

        #region Six Channels

        TValue[,] ToArray<TValue>(IList<Tuple<TValue, TValue, TValue, TValue, TValue, TValue>> source)
        {
            var output = new TValue[6, source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                var tuple = source[i];
                output[0, i] = tuple.Item1;
                output[1, i] = tuple.Item2;
                output[2, i] = tuple.Item3;
                output[3, i] = tuple.Item4;
                output[4, i] = tuple.Item5;
                output[5, i] = tuple.Item6;
            }
            return output;
        }

        public IObservable<Mat> Process(IObservable<Tuple<byte, byte, byte, byte, byte, byte>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short, short, short, short, short>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort, ushort, ushort, ushort, ushort>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int, int, int, int, int>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float, float, float, float, float>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double, double, double, double, double>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        #endregion

        #region Seven Channels

        TValue[,] ToArray<TValue>(IList<Tuple<TValue, TValue, TValue, TValue, TValue, TValue, TValue>> source)
        {
            var output = new TValue[7, source.Count];
            for (int i = 0; i < source.Count; i++)
            {
                var tuple = source[i];
                output[0, i] = tuple.Item1;
                output[1, i] = tuple.Item2;
                output[2, i] = tuple.Item3;
                output[3, i] = tuple.Item4;
                output[4, i] = tuple.Item5;
                output[5, i] = tuple.Item6;
                output[6, i] = tuple.Item7;
            }
            return output;
        }

        public IObservable<Mat> Process(IObservable<Tuple<byte, byte, byte, byte, byte, byte, byte>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short, short, short, short, short, short>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort, ushort, ushort, ushort, ushort, ushort>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int, int, int, int, int, int>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float, float, float, float, float, float>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double, double, double, double, double, double>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        #endregion

        #region N-Channels

        TValue[,] ToArray<TValue>(IList<IList<TValue>> source)
        {
            if (source.Count == 0)
            {
                return new TValue[0, 0];
            }

            var channels = source[0].Count;
            var output = new TValue[channels, source.Count];
            for (int j = 0; j < source.Count; j++)
            {
                var list = source[j];
                if (list.Count != channels)
                {
                    throw new InvalidOperationException("All samples in the buffer must have the same number of channels.");
                }

                for (int i = 0; i < list.Count; i++)
                {
                    output[i, j] = list[i]; 
                }
            }
            return output;
        }

        public IObservable<Mat> Process(IObservable<IList<byte>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<IList<short>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<IList<ushort>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<IList<int>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<IList<float>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<IList<double>> source)
        {
            var skip = Skip;
            var sourceBuffer = skip.HasValue ? source.Buffer(Count, skip.Value) : source.Buffer(Count);
            return sourceBuffer.Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Create<Mat>(observer =>
            {
                var skipCount = 0;
                var count = Count;
                var skip = Skip.GetValueOrDefault(count);
                var activeBuffers = new List<SampleBuffer>();
                return source.Subscribe(input =>
                {
                    try
                    {
                        // Update pending windows
                        activeBuffers.RemoveAll(buffer =>
                        {
                            buffer.Update(input, 0);
                            if (buffer.Completed)
                            {
                                // Window is ready, emit
                                observer.OnNext(buffer.Samples);
                                return true;
                            }

                            return false;
                        });

                        var index = 0;
                        while ((index + skipCount) < input.Cols)
                        {
                            // Create new window and reset skip counter
                            index += skipCount;
                            skipCount = skip;
                            var buffer = new SampleBuffer(input, count, index);
                            buffer.Update(input, index);
                            if (buffer.Completed)
                            {
                                // Window is ready, emit
                                observer.OnNext(buffer.Samples);
                            }
                            // Window is missing data, add to list
                            else activeBuffers.Add(buffer);
                        }

                        // Remove remainder of input samples from skip counter
                        skipCount -= input.Cols - index;
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                observer.OnError,
                () =>
                {
                    // Emit pending windows
                    activeBuffers.RemoveAll(buffer =>
                    {
                        var remainder = new Rect(0, 0, (int)buffer.SampleIndex, buffer.Samples.Rows);
                        observer.OnNext(buffer.Samples.GetSubRect(remainder));
                        return true;
                    });

                    observer.OnCompleted();
                });
            });
        }

        #endregion
    }
}
