using OpenCV.Net;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that calculates the absolute value of each element in the sequence.
    /// </summary>
    [Description("Calculates the absolute value of each element in the sequence.")]
    public class Abs : ArrayTransform
    {
        /// <summary>
        /// Calculates the absolute value of each 8-bit signed integer in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 8-bit signed integer values.
        /// </param>
        /// <returns>
        /// A sequence of 8-bit signed integer values, where each value
        /// is greater than or equal to zero.
        /// </returns>
        public IObservable<sbyte> Process(IObservable<sbyte> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        /// <summary>
        /// Calculates the absolute value of each 16-bit signed integer in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 16-bit signed integer values.
        /// </param>
        /// <returns>
        /// A sequence of 16-bit signed integer values, where each value
        /// is greater than or equal to zero.
        /// </returns>
        public IObservable<short> Process(IObservable<short> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        /// <summary>
        /// Calculates the absolute value of each 32-bit signed integer in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 32-bit signed integer values.
        /// </param>
        /// <returns>
        /// A sequence of 32-bit signed integer values, where each value
        /// is greater than or equal to zero.
        /// </returns>
        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        /// <summary>
        /// Calculates the absolute value of each 64-bit signed integer in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 64-bit signed integer values.
        /// </param>
        /// <returns>
        /// A sequence of 64-bit signed integer values, where each value
        /// is greater than or equal to zero.
        /// </returns>
        public IObservable<long> Process(IObservable<long> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        /// <summary>
        /// Calculates the absolute value of each 32-bit floating-point number in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 32-bit floating-point numbers.
        /// </param>
        /// <returns>
        /// A sequence of 32-bit floating-point numbers, where each value
        /// is greater than or equal to zero.
        /// </returns>
        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        /// <summary>
        /// Calculates the absolute value of each 64-bit floating-point number in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 64-bit floating-point numbers.
        /// </param>
        /// <returns>
        /// A sequence of 64-bit floating-point numbers, where each value
        /// is greater than or equal to zero.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        /// <summary>
        /// Calculates the absolute value of each <see cref="decimal"/> number in the sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="decimal"/> numbers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="decimal"/> numbers, where each value
        /// is greater than or equal to zero.
        /// </returns>
        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Select(input => Math.Abs(input));
        }

        /// <summary>
        /// Calculates the absolute value of individual elements for all arrays in the sequence.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of multi-channel array values.
        /// </param>
        /// <returns>
        /// A sequence of multi-channel array values, where each element of the array
        /// is greater than or equal to zero.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            return source.Select(input =>
            {
                var output = outputFactory(input);
                CV.Abs(input, output);
                return output;
            });
        }
    }
}
