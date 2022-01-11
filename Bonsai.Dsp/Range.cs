using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that generates a sequence of buffers filled with a
    /// specified range of numbers.
    /// </summary>
    [Description("Generates a sequence of buffers filled with a specified range of numbers.")]
    public class Range : Source<Mat>
    {
        /// <summary>
        /// Gets or sets the number of samples in each output buffer.
        /// </summary>
        [Description("The number of samples in each output buffer.")]
        public int BufferLength { get; set; }

        /// <summary>
        /// Gets or sets the bit depth of each element in the output buffer.
        /// </summary>
        [TypeConverter(typeof(DepthConverter))]
        [Description("The bit depth of each element in the output buffer.")]
        public Depth Depth { get; set; } = Depth.F32;

        /// <summary>
        /// Gets or sets the inclusive lower bound of the range.
        /// </summary>
        [Description("The inclusive lower bound of the range.")]
        public double Start { get; set; }

        /// <summary>
        /// Gets or sets the exclusive upper bound of the range.
        /// </summary>
        [Description("The exclusive upper bound of the range.")]
        public double End { get; set; }

        Mat CreateBuffer()
        {
            var buffer = new Mat(1, BufferLength, Depth, 1);
            CV.Range(buffer, Start, End);
            return buffer;
        }

        /// <summary>
        /// Generates an observable sequence of buffers filled with a
        /// specified range of numbers.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing fixed-size buffers
        /// linearly filled with values between the inclusive lower bound and
        /// exclusive upper bound.
        /// </returns>
        public override IObservable<Mat> Generate()
        {
            return Observable.Defer(() => Observable.Return(CreateBuffer()));
        }

        /// <summary>
        /// Generates an observable sequence of buffers filled with a specified
        /// range of numbers, and where each new buffer is emitted only when an
        /// observable sequence raises a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting sample buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing fixed-size buffers
        /// linearly filled with values between the inclusive lower bound and
        /// exclusive upper bound.
        /// </returns>
        public IObservable<Mat> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => CreateBuffer());
        }

        class DepthConverter : EnumConverter
        {
            public DepthConverter(Type type)
                : base(type)
            {
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] { Depth.S32, Depth.F32 });
            }
        }
    }
}
