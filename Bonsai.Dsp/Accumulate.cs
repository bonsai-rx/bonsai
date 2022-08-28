using System;
using System.Linq;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that calculates the cumulative sum of the arrays in a sequence
    /// and returns each intermediate result.
    /// </summary>
    [Description("Calculates the cumulative sum of the arrays in a sequence and returns each intermediate result.")]
    public class Accumulate : ArrayTransform
    {
        /// <summary>
        /// Calculates the cumulative sum of the arrays in an observable sequence and returns
        /// each intermediate result.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of multi-channel array values, where each array stores the
        /// cumulative sum of all previous array values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            var accumulatorFactory = ArrFactory<TArray>.TemplateSizeChannelFactory;
            return Observable.Defer(() =>
            {
                TArray accumulator = null;
                return source.Select(input =>
                {
                    if (accumulator == null)
                    {
                        accumulator = accumulatorFactory(input, Depth.F32);
                        CV.Convert(input, accumulator);
                        return input;
                    }
                    else
                    {
                        var output = outputFactory(input);
                        CV.Acc(input, accumulator);
                        CV.Convert(accumulator, output);
                        return output;
                    }
                });
            });
        }
    }
}
