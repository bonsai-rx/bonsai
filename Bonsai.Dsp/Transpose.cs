using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that transposes every array in the sequence.
    /// </summary>
    [Description("Transposes every array in the sequence.")]
    public class Transpose : ArrayTransform
    {
        /// <summary>
        /// Transposes every array in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of multi-channel array values, where each new array is the
        /// transpose of the original array.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateDepthChannelFactory;
            return source.Select(input =>
            {
                var inputSize = input.Size;
                var output = outputFactory(input, new Size(inputSize.Height, inputSize.Width));
                CV.Transpose(input, output);
                return output;
            });
        }
    }
}
