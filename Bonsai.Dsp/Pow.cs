using OpenCV.Net;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that raises every element in the sequence to the specified power.
    /// </summary>
    [Description("Raises every element in the sequence to the specified power.")]
    public class Pow : ArrayTransform
    {
        /// <summary>
        /// Gets or sets the specified power.
        /// </summary>
        [Description("Specifies the power.")]
        public double Power { get; set; }

        /// <summary>
        /// Raises every 64-bit floating-point number in an observable sequence
        /// to the specified power.
        /// </summary>
        /// <param name="source">
        /// A sequence of 64-bit floating-point numbers.
        /// </param>
        /// <returns>
        /// A sequence of 64-bit floating-point numbers, where each value
        /// represents a number raised to the specified power. See
        /// <see cref="Math.Pow(double, double)"/>.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Select(input => Math.Pow(input, Power));
        }

        /// <summary>
        /// Raises every individual element for all arrays in an observable sequence
        /// to the specified power.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of multi-channel array values, where each element of the array
        /// represents a value raised to the specified power.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var output = outputFactory(input);
                CV.Pow(input, output, Power);
                return output;
            });
        }
    }
}
