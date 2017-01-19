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
    [Description("Demultiplexes individual bits of the input elements into independent channels.")]
    public class BitBuffer
    {
        public IObservable<Mat> Process(IObservable<byte> source)
        {
            return Observable.Defer(() =>
            {
                var output = new byte[8];
                return source.Select(input =>
                {
                    for (int i = 0; i < output.Length; i++)
                    {
                        output[i] = (byte)(input >> i & 0x1);
                    }
                    return Mat.FromArray(output, output.Length, 1, Depth.U8, 1);
                });
            });
        }

        public IObservable<Mat> Process(IObservable<sbyte> source)
        {
            return Observable.Defer(() =>
            {
                var output = new byte[8];
                return source.Select(input =>
                {
                    for (int i = 0; i < output.Length; i++)
                    {
                        output[i] = (byte)(input >> i & 0x1);
                    }
                    return Mat.FromArray(output, output.Length, 1, Depth.U8, 1);
                });
            });
        }

        public IObservable<Mat> Process(IObservable<short> source)
        {
            return Observable.Defer(() =>
            {
                var output = new byte[16];
                return source.Select(input =>
                {
                    for (int i = 0; i < output.Length; i++)
                    {
                        output[i] = (byte)(input >> i & 0x1);
                    }
                    return Mat.FromArray(output, output.Length, 1, Depth.U8, 1);
                });
            });
        }

        public IObservable<Mat> Process(IObservable<ushort> source)
        {
            return Observable.Defer(() =>
            {
                var output = new byte[16];
                return source.Select(input =>
                {
                    for (int i = 0; i < output.Length; i++)
                    {
                        output[i] = (byte)(input >> i & 0x1);
                    }
                    return Mat.FromArray(output, output.Length, 1, Depth.U8, 1);
                });
            });
        }

        public IObservable<Mat> Process(IObservable<int> source)
        {
            return Observable.Defer(() =>
            {
                var output = new byte[32];
                return source.Select(input =>
                {
                    for (int i = 0; i < output.Length; i++)
                    {
                        output[i] = (byte)(input >> i & 0x1);
                    }
                    return Mat.FromArray(output, output.Length, 1, Depth.U8, 1);
                });
            });
        }

        public IObservable<Mat> Process(IObservable<uint> source)
        {
            return Observable.Defer(() =>
            {
                var output = new byte[32];
                return source.Select(input =>
                {
                    for (int i = 0; i < output.Length; i++)
                    {
                        output[i] = (byte)(input >> i & 0x1);
                    }
                    return Mat.FromArray(output, output.Length, 1, Depth.U8, 1);
                });
            });
        }

        public IObservable<Mat> Process(IObservable<long> source)
        {
            return Observable.Defer(() =>
            {
                var output = new byte[64];
                return source.Select(input =>
                {
                    for (int i = 0; i < output.Length; i++)
                    {
                        output[i] = (byte)(input >> i & 0x1);
                    }
                    return Mat.FromArray(output, output.Length, 1, Depth.U8, 1);
                });
            });
        }

        public IObservable<Mat> Process(IObservable<ulong> source)
        {
            return Observable.Defer(() =>
            {
                var output = new byte[64];
                return source.Select(input =>
                {
                    for (int i = 0; i < output.Length; i++)
                    {
                        output[i] = (byte)(input >> i & 0x1);
                    }
                    return Mat.FromArray(output, output.Length, 1, Depth.U8, 1);
                });
            });
        }

        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                if (input.Rows > 1)
                {
                    throw new InvalidOperationException("The input buffer must have a single channel.");
                }

                var bitbuffer = new Mat(input.ElementSize * 8, input.Cols, input.Depth, 1);
                for (int i = 0; i < bitbuffer.Rows; i++)
                {
                    using (var row = bitbuffer.GetRow(i))
                    {
                        CV.AndS(input, Scalar.Real(1 << i), row);
                    }
                }

                var output = input.Depth != Depth.U8 ? new Mat(bitbuffer.Size, Depth.U8, 1) : bitbuffer;
                if (output != bitbuffer) CV.CmpS(bitbuffer, 0, output, ComparisonOperation.NotEqual);
                CV.Threshold(output, output, 1, 1, ThresholdTypes.Truncate);
                return output;
            });
        }
    }
}
