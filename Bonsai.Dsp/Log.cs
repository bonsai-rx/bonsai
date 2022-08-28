using OpenCV.Net;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that calculates the natural logarithm of the absolute
    /// value of every element in the sequence.
    /// </summary>
    [Description("Calculates the natural logarithm of the absolute value of every element in the sequence.")]
    public class Log : ArrayTransform
    {
        /// <summary>
        /// Calculates the natural logarithm of each 64-bit floating-point number in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 64-bit floating-point numbers.
        /// </param>
        /// <returns>
        /// A sequence of 64-bit floating-point numbers, where each value represents the
        /// natural logarithm of the corresponding number in the <paramref name="source"/>
        /// sequence. See <see cref="Math.Log(double)"/>.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Select(input => Math.Log(input));
        }

        /// <summary>
        /// Calculates the natural logarithm of the absolute value of every element for
        /// each array in the sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of multi-channel array values, where each element of the array
        /// represents the natural logarithm of the corresponding element in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var output = outputFactory(input);
                CV.Log(input, output);
                return output;
            });
        }
    }
}
