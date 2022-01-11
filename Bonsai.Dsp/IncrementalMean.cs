using System;
using System.Linq;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that incrementally computes the mean of the arrays in the sequence
    /// and returns each intermediate result.
    /// </summary>
    [Description("Incrementally computes the mean of the arrays in the sequence and returns each intermediate result.")]
    public class IncrementalMean : ArrayTransform
    {
        /// <summary>
        /// Incrementally computes the mean of the arrays in an observable sequence
        /// and returns each intermediate result.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of multi-channel array values, where each array stores the
        /// incremental mean of all previous array values in the <paramref name="source"/>
        /// sequence.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            return Observable.Defer(() =>
            {
                var count = 0;
                TArray mean = null;
                var outputFactory = ArrFactory<TArray>.TemplateFactory;
                return source.Select(input =>
                {
                    if (mean == null)
                    {
                        mean = outputFactory(input);
                        mean.SetZero();
                    }

                    var output = outputFactory(input);
                    CV.Sub(input, mean, output);
                    CV.ConvertScale(output, output, 1f / ++count, 0);
                    CV.Add(mean, output, output);
                    mean = output;
                    return output;
                });
            });

        }
    }
}
