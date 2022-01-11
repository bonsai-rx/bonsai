using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that reduces every array in the sequence to a 1D vector using the specified operation.
    /// </summary>
    [Description("Reduces every array in the sequence to a 1D vector using the specified operation.")]
    public class Reduce : ArrayTransform
    {
        /// <summary>
        /// Gets or sets the dimension along which to reduce the array.
        /// </summary>
        [Description("The dimension along which to reduce the array.")]
        public int Axis { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the reduction operation to be applied.
        /// </summary>
        [Description("Specifies the reduction operation to be applied.")]
        public ReduceOperation Operation { get; set; }

        /// <summary>
        /// Reduces every array in an observable sequence to a 1D vector using
        /// the specified operation.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of 1D vector arrays storing the results of the reduction operation.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateDepthChannelFactory;
            return source.Select(input =>
            {
                if (Axis < 0 || Axis > 1)
                {
                    throw new InvalidOperationException("The axis dimension must be either 0 (single row) or 1 (single column).");
                }

                var inputSize = input.Size;
                var outputWidth = Axis == 1 ? 1 : inputSize.Width;
                var outputHeight = Axis == 0 ? 1 : inputSize.Height;
                var outputSize = new Size(outputWidth, outputHeight);
                var output = outputFactory(input, outputSize);
                CV.Reduce(input, output, Axis, Operation);
                return output;
            });
        }
    }
}
