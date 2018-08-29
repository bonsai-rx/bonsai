using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts each managed array in the sequence into a buffer with the specified size, depth and number of channels.")]
    public class ConvertFromArray
    {
        [Description("The size of the output buffer.")]
        public Size Size { get; set; }

        [Description("The optional bit depth of each element in the output buffer.")]
        public Depth? Depth { get; set; }

        [Description("The optional number of channels in the output buffer.")]
        public int? Channels { get; set; }

        static int ElementSize(Depth depth)
        {
            switch (depth)
            {
                case OpenCV.Net.Depth.U8:
                case OpenCV.Net.Depth.S8: return 1;
                case OpenCV.Net.Depth.U16:
                case OpenCV.Net.Depth.S16: return 2;
                case OpenCV.Net.Depth.S32:
                case OpenCV.Net.Depth.F32: return 4;
                case OpenCV.Net.Depth.F64: return 8;
                default: throw new ArgumentException("Invalid depth was specified.");
            }
        }

        Mat FromArray<TData>(TData[] input, Depth? defaultDepth) where TData : struct
        {
            var size = Size;
            var depth = Depth;
            var channels = Channels;
            if (!defaultDepth.HasValue && !depth.HasValue)
            {
                throw new InvalidOperationException("Depth must be specified when converting arrays with custom types.");
            }

            if (size.Width > 0 || size.Height > 0 || depth.HasValue || channels.HasValue)
            {
                var elementSize = Marshal.SizeOf(typeof(TData));
                if (!depth.HasValue) depth = defaultDepth;
                if (!channels.HasValue) channels = 1;

                var rows = size.Height;
                var cols = size.Width;
                if (rows == 0 && cols == 0) rows = 1;
                if (rows == 0) rows = input.Length * elementSize / (ElementSize(depth.Value) * channels.Value * cols);
                if (cols == 0) cols = input.Length * elementSize / (ElementSize(depth.Value) * channels.Value * rows);
                return Mat.FromArray(input, rows, cols, depth.Value, channels.Value);
            }
            else return null;
        }

        public IObservable<Mat> Process(IObservable<byte[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.U8);
                return output ?? Mat.FromArray(input);
            });
        }

        public IObservable<Mat> Process(IObservable<short[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.S16);
                return output ?? Mat.FromArray(input);
            });
        }

        public IObservable<Mat> Process(IObservable<ushort[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.U16);
                return output ?? Mat.FromArray(input);
            });
        }

        public IObservable<Mat> Process(IObservable<int[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.S32);
                return output ?? Mat.FromArray(input);
            });
        }

        public IObservable<Mat> Process(IObservable<float[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.F32);
                return output ?? Mat.FromArray(input);
            });
        }

        public IObservable<Mat> Process(IObservable<double[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.F64);
                return output ?? Mat.FromArray(input);
            });
        }

        public IObservable<Mat> Process<TData>(IObservable<TData[]> source) where TData : struct
        {
            return source.Select(input => FromArray(input, null));
        }
    }
}
