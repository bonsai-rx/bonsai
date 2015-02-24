using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class Buffer
    {
        public int Count { get; set; }

        #region One Channel

        public IObservable<Mat> Process(IObservable<byte> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<short> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<ushort> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<int> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<float> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
        }

        public IObservable<Mat> Process(IObservable<double> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(buffer.ToArray()));
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
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
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
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short, short>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort, ushort>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int, int>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float, float>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double, double>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
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
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short, short, short>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort, ushort, ushort>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int, int, int>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float, float, float>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double, double, double>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
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
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short, short, short, short>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort, ushort, ushort, ushort>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int, int, int, int>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float, float, float, float>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double, double, double, double>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
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
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short, short, short, short, short>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort, ushort, ushort, ushort, ushort>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int, int, int, int, int>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float, float, float, float, float>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double, double, double, double, double>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
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
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<short, short, short, short, short, short, short>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<ushort, ushort, ushort, ushort, ushort, ushort, ushort>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<int, int, int, int, int, int, int>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<float, float, float, float, float, float, float>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        public IObservable<Mat> Process(IObservable<Tuple<double, double, double, double, double, double, double>> source)
        {
            return source.Buffer(Count).Select(buffer => Mat.FromArray(ToArray(buffer)));
        }

        #endregion

        #region N-Channels

        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Create<Mat>(observer =>
            {
                var bufferIndex = 0;
                var buffer = default(Mat);
                return source.Subscribe(input =>
                {
                    try
                    {
                        var inputIndex = 0;
                        while (inputIndex < input.Cols)
                        {
                            if (buffer == null)
                            {
                                bufferIndex = 0;
                                buffer = new Mat(input.Rows, Count, input.Depth, input.Channels);
                            }

                            var samplesTaken = Math.Min(input.Cols - inputIndex, buffer.Cols - bufferIndex);
                            using (var bufferRoi = buffer.GetSubRect(new Rect(bufferIndex, 0, samplesTaken, buffer.Rows)))
                            using (var inputRoi = input.GetSubRect(new Rect(inputIndex, 0, samplesTaken, input.Rows)))
                            {
                                CV.Copy(inputRoi, bufferRoi);
                                bufferIndex += samplesTaken;
                                inputIndex += samplesTaken;
                                if (bufferIndex >= buffer.Cols)
                                {
                                    observer.OnNext(buffer);
                                    buffer = null;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                observer.OnError,
                () =>
                {
                    try
                    {
                        if (buffer != null)
                        {
                            var lastBuffer = new Mat(buffer.Rows, bufferIndex, buffer.Depth, buffer.Channels);
                            using (var bufferRoi = buffer.GetSubRect(new Rect(0, 0, bufferIndex, buffer.Rows)))
                            {
                                CV.Copy(bufferRoi, lastBuffer);
                            }

                            observer.OnNext(lastBuffer);
                            buffer = null;
                        }

                        observer.OnCompleted();
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                });
            });
        }

        #endregion
    }
}
