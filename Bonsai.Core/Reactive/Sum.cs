using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that computes the sum of an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Computes the sum of an observable sequence.")]
    public class Sum
    {
        /// <summary>
        /// Computes the sum of a sequence of nullable <see cref="decimal"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="decimal"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the sum
        /// of the values in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<decimal?> Process(IObservable<decimal?> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="decimal"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="decimal"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the sum
        /// of the values in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of nullable <see cref="double"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="double"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the sum
        /// of the values in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double?> Process(IObservable<double?> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="double"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="double"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the sum
        /// of the values in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of nullable <see cref="float"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="float"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the sum
        /// of the values in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<float?> Process(IObservable<float?> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="float"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="float"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the sum
        /// of the values in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of nullable <see cref="int"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="int"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the sum
        /// of the values in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<int?> Process(IObservable<int?> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="int"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="int"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the sum
        /// of the values in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of nullable <see cref="long"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="long"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the sum
        /// of the values in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<long?> Process(IObservable<long?> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="long"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="long"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the sum
        /// of the values in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<long> Process(IObservable<long> source)
        {
            return source.Sum();
        }
    }
}
