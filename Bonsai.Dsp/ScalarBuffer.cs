using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that generates a sequence with a single buffer
    /// where all elements are set to the same scalar value.
    /// </summary>
    [Description("Generates a sequence with a single buffer where all elements are set to the same scalar value.")]
    public class ScalarBuffer : Source<Mat>
    {
        /// <summary>
        /// Gets or sets the size of the output buffer.
        /// </summary>
        [Description("The size of the output buffer.")]
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets the bit depth of each element in the output buffer.
        /// </summary>
        [Description("The bit depth of each element in the output buffer.")]
        public Depth Depth { get; set; } = Depth.F32;

        /// <summary>
        /// Gets or sets the number of channels in the output buffer.
        /// </summary>
        [Description("The number of channels in the output buffer.")]
        public int Channels { get; set; } = 1;

        /// <summary>
        /// Gets or sets the scalar value to which all elements in the output buffer
        /// will be set to.
        /// </summary>
        [Description("The scalar value to which all elements in the output buffer will be set to.")]
        public Scalar Value { get; set; }

        Mat CreateBuffer()
        {
            var buffer = new Mat(Size, Depth, Channels);
            buffer.Set(Value);
            return buffer;
        }

        /// <summary>
        /// Generates an observable sequence with a single buffer where all elements
        /// are set to the same scalar value.
        /// </summary>
        /// <returns>
        /// A sequence with a single <see cref="Mat"/> object with the specified
        /// element type and where all the elements are set to the same scalar value.
        /// </returns>
        public override IObservable<Mat> Generate()
        {
            return Observable.Defer(() => Observable.Return(CreateBuffer()));
        }

        /// <summary>
        /// Generates an observable sequence of buffers where all elements
        /// are set to the same scalar value, and where each new buffer is
        /// emitted only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects with the specified element type
        /// and where all the elements are set to the same scalar value.
        /// </returns>
        public IObservable<Mat> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => CreateBuffer());
        }
    }
}
