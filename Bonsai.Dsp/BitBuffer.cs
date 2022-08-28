using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that demultiplexes individual bits of all the elements in a sequence
    /// into separate rows of a 2D array.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Demultiplexes individual bits of all the elements in a sequence into separate rows of a 2D array.")]
    public class BitBuffer
    {
        /// <summary>
        /// Demultiplexes individual bits of all the 8-bit unsigned integers in a sequence
        /// into eight separate rows of a 2D array.
        /// </summary>
        /// <param name="source">
        /// A sequence of 8-bit unsigned integer values.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects, where each array contains one row
        /// for each bit in the 8-bit unsigned integer value.
        /// </returns>
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

        /// <summary>
        /// Demultiplexes individual bits of all the 8-bit signed integers in a sequence
        /// into eight separate rows of a 2D array.
        /// </summary>
        /// <param name="source">
        /// A sequence of 8-bit signed integer values.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects, where each array contains one row
        /// for each bit in the 8-bit signed integer value.
        /// </returns>
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

        /// <summary>
        /// Demultiplexes individual bits of all the 16-bit signed integers in a sequence
        /// into sixteen separate rows of a 2D array.
        /// </summary>
        /// <param name="source">
        /// A sequence of 16-bit signed integer values.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects, where each array contains one row
        /// for each bit in the 16-bit signed integer value.
        /// </returns>
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

        /// <summary>
        /// Demultiplexes individual bits of all the 16-bit unsigned integers in a sequence
        /// into sixteen separate rows of a 2D array.
        /// </summary>
        /// <param name="source">
        /// A sequence of 16-bit unsigned integer values.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects, where each array contains one row
        /// for each bit in the 16-bit unsigned integer value.
        /// </returns>
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

        /// <summary>
        /// Demultiplexes individual bits of all the 32-bit signed integers in a sequence
        /// into thirty-two separate rows of a 2D array.
        /// </summary>
        /// <param name="source">
        /// A sequence of 32-bit signed integer values.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects, where each array contains one row
        /// for each bit in the 32-bit signed integer value.
        /// </returns>
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

        /// <summary>
        /// Demultiplexes individual bits of all the 32-bit unsigned integers in a sequence
        /// into thirty-two separate rows of a 2D array.
        /// </summary>
        /// <param name="source">
        /// A sequence of 32-bit unsigned integer values.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects, where each array contains one row
        /// for each bit in the 32-bit unsigned integer value.
        /// </returns>
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

        /// <summary>
        /// Demultiplexes individual bits of all the 64-bit signed integers in a sequence
        /// into sixty-four separate rows of a 2D array.
        /// </summary>
        /// <param name="source">
        /// A sequence of 64-bit signed integer values.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects, where each array contains one row
        /// for each bit in the 64-bit signed integer value.
        /// </returns>
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

        /// <summary>
        /// Demultiplexes individual bits of all the 64-bit unsigned integers in a sequence
        /// into sixty-four separate rows of a 2D array.
        /// </summary>
        /// <param name="source">
        /// A sequence of 64-bit unsigned integer values.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects, where each array contains one row
        /// for each bit in the 64-bit unsigned integer value.
        /// </returns>
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

        /// <summary>
        /// Demultiplexes individual bits of all the 2D array values in a sequence
        /// into multiple separate rows of a 2D array.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D array values.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects, where each array contains one row
        /// for each bit in the original 2D array value.
        /// </returns>
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
