using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that calculates the absolute difference between all pairs of arrays in the sequence.
    /// </summary>
    [Description("Calculates the absolute difference between all pairs of arrays in the sequence.")]
    public class AbsoluteDifference : BinaryArrayTransform
    {
        /// <summary>
        /// Calculates the absolute difference between all pairs of arrays in an observable sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of multi-channel array values, where each array stores the absolute
        /// difference between each pair in the <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var first = input.Item1;
                var second = input.Item2;
                var output = outputFactory(first);
                CV.AbsDiff(first, second, output);
                return output;
            });
        }
    }
}
