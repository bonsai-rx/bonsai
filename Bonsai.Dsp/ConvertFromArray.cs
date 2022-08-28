using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that converts each managed array in the sequence into a
    /// 2D array buffer with the specified size, depth and number of channels.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts each managed array in the sequence into a 2D array buffer with the specified size, depth and number of channels.")]
    public class ConvertFromArray
    {
        /// <summary>
        /// Gets or sets the size of the output array buffer.
        /// </summary>
        /// <remarks>
        /// If one dimension is zero, the output will be either a row or column vector
        /// along the non-zero dimension. If both dimensions are zero, the output will
        /// be a row vector with the same number of elements as the length of each
        /// array in the sequence.
        /// </remarks>
        [Description("The size of the output array buffer.")]
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the bit depth of each element in the output array buffer.
        /// </summary>
        /// <remarks>
        /// If this property is not specified, the default depth will be automatically
        /// selected based on the type of the input array elements.
        /// </remarks>
        [Description("The optional bit depth of each element in the output array buffer.")]
        public Depth? Depth { get; set; }

        /// <summary>
        /// Gets or sets the number of channels in the output array buffer.
        /// </summary>
        /// <remarks>
        /// If this property is not specified, the default number of channels
        /// will be one.
        /// </remarks>
        [Description("The optional number of channels in the output array buffer.")]
        public int? Channels { get; set; }

        Mat FromArray<TData>(TData[] input, Depth? defaultDepth) where TData : struct
        {
            var size = Size;
            var depth = Depth;
            var channels = Channels;
            if (!defaultDepth.HasValue && !depth.HasValue)
            {
                throw new InvalidOperationException("Depth must be specified when converting arrays with custom types.");
            }

            if (!depth.HasValue) depth = defaultDepth;
            if (size.Width > 0 || size.Height > 0 || channels.HasValue)
            {
                var elementSize = Marshal.SizeOf(typeof(TData));
                if (!channels.HasValue) channels = 1;

                var rows = size.Height;
                var cols = size.Width;
                if (rows == 0 && cols == 0) rows = 1;
                if (rows == 0) rows = input.Length * elementSize / (ArrHelper.ElementSize(depth.Value) * channels.Value * cols);
                if (cols == 0) cols = input.Length * elementSize / (ArrHelper.ElementSize(depth.Value) * channels.Value * rows);
                return Mat.FromArray(input, rows, cols, depth.Value, channels.Value);
            }
            else return null;
        }

        /// <summary>
        /// Converts each <see cref="byte"/> array in an observable sequence into a
        /// 2D array buffer with the specified size, depth and number of channels.
        /// </summary>
        /// <param name="source">
        /// A sequence of 8-bit unsigned integer arrays to convert into a sequence of
        /// sample buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects containing a copy of the managed
        /// array data reinterpreted as a 2D array buffer with the specified size, depth
        /// and number of channels.
        /// </returns>
        public IObservable<Mat> Process(IObservable<byte[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.U8);
                return output ?? Mat.FromArray(input);
            });
        }

        /// <summary>
        /// Converts each <see cref="short"/> array in an observable sequence into a
        /// 2D array buffer with the specified size, depth and number of channels.
        /// </summary>
        /// <param name="source">
        /// A sequence of 16-bit signed integer arrays to convert into a sequence of
        /// sample buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects containing a copy of the managed
        /// array data reinterpreted as a 2D array buffer with the specified size, depth
        /// and number of channels.
        /// </returns>
        public IObservable<Mat> Process(IObservable<short[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.S16);
                return output ?? Mat.FromArray(input);
            });
        }

        /// <summary>
        /// Converts each <see cref="ushort"/> array in an observable sequence into a
        /// 2D array buffer with the specified size, depth and number of channels.
        /// </summary>
        /// <param name="source">
        /// A sequence of 16-bit unsigned integer arrays to convert into a sequence of
        /// sample buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects containing a copy of the managed
        /// array data reinterpreted as a 2D array buffer with the specified size, depth
        /// and number of channels.
        /// </returns>
        public IObservable<Mat> Process(IObservable<ushort[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.U16);
                return output ?? Mat.FromArray(input);
            });
        }

        /// <summary>
        /// Converts each <see cref="int"/> array in an observable sequence into a
        /// 2D array buffer with the specified size, depth and number of channels.
        /// </summary>
        /// <param name="source">
        /// A sequence of 32-bit signed integer arrays to convert into a sequence of
        /// sample buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects containing a copy of the managed
        /// array data reinterpreted as a 2D array buffer with the specified size, depth
        /// and number of channels.
        /// </returns>
        public IObservable<Mat> Process(IObservable<int[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.S32);
                return output ?? Mat.FromArray(input);
            });
        }

        /// <summary>
        /// Converts each <see cref="float"/> array in an observable sequence into a
        /// 2D array buffer with the specified size, depth and number of channels.
        /// </summary>
        /// <param name="source">
        /// A sequence of 32-bit floating-point arrays to convert into a sequence of
        /// sample buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects containing a copy of the managed
        /// array data reinterpreted as a 2D array buffer with the specified size, depth
        /// and number of channels.
        /// </returns>
        public IObservable<Mat> Process(IObservable<float[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.F32);
                return output ?? Mat.FromArray(input);
            });
        }

        /// <summary>
        /// Converts each <see cref="double"/> array in an observable sequence into a
        /// 2D array buffer with the specified size, depth and number of channels.
        /// </summary>
        /// <param name="source">
        /// A sequence of 64-bit floating-point arrays to convert into a sequence of
        /// sample buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects containing a copy of the managed
        /// array data reinterpreted as a 2D array buffer with the specified size, depth
        /// and number of channels.
        /// </returns>
        public IObservable<Mat> Process(IObservable<double[]> source)
        {
            return source.Select(input =>
            {
                var output = FromArray(input, OpenCV.Net.Depth.F64);
                return output ?? Mat.FromArray(input);
            });
        }

        /// <summary>
        /// Converts each array of type <typeparamref name="TData"/> in an observable
        /// sequence into a 2D array buffer with the specified size, depth and number
        /// of channels.
        /// </summary>
        /// <typeparam name="TData">The type of the values stored in each array.</typeparam>
        /// <param name="source">
        /// A sequence of arrays of type <typeparamref name="TData"/> to convert into a
        /// sequence of sample buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects containing a copy of the managed
        /// array data reinterpreted as a 2D array buffer with the specified size, depth
        /// and number of channels.
        /// </returns>
        public IObservable<Mat> Process<TData>(IObservable<TData[]> source) where TData : struct
        {
            return source.Select(input => FromArray(input, null));
        }
    }
}
