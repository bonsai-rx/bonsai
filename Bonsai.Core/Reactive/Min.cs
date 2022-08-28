using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that computes the minimum element in an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Computes the minimum element in an observable sequence.")]
    public class Min
    {
        /// <summary>
        /// Returns the minimum value in an observable sequence of nullable
        /// <see cref="decimal"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="decimal"/> values to determine the
        /// minimum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<decimal?> Process(IObservable<decimal?> source)
        {
            return source.Min();
        }

        /// <summary>
        /// Returns the minimum value in an observable sequence of <see cref="decimal"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="decimal"/> values to determine the minimum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Min();
        }

        /// <summary>
        /// Returns the minimum value in an observable sequence of nullable
        /// <see cref="double"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="double"/> values to determine the
        /// minimum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double?> Process(IObservable<double?> source)
        {
            return source.Min();
        }

        /// <summary>
        /// Returns the minimum value in an observable sequence of <see cref="double"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="double"/> values to determine the minimum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Min();
        }

        /// <summary>
        /// Returns the minimum value in an observable sequence of nullable
        /// <see cref="float"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="float"/> values to determine the
        /// minimum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<float?> Process(IObservable<float?> source)
        {
            return source.Min();
        }

        /// <summary>
        /// Returns the minimum value in an observable sequence of <see cref="float"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="float"/> values to determine the minimum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Min();
        }

        /// <summary>
        /// Returns the minimum value in an observable sequence of nullable
        /// <see cref="int"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="int"/> values to determine the
        /// minimum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<int?> Process(IObservable<int?> source)
        {
            return source.Min();
        }

        /// <summary>
        /// Returns the minimum value in an observable sequence of <see cref="int"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="int"/> values to determine the minimum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Min();
        }

        /// <summary>
        /// Returns the minimum value in an observable sequence of nullable
        /// <see cref="long"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="long"/> values to determine the
        /// minimum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<long?> Process(IObservable<long?> source)
        {
            return source.Min();
        }

        /// <summary>
        /// Returns the minimum value in an observable sequence of <see cref="long"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="long"/> values to determine the minimum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<long> Process(IObservable<long> source)
        {
            return source.Min();
        }

        /// <summary>
        /// Returns the minimum element in an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// An observable sequence to determine the minimum element of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// minimum value in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Min();
        }
    }
}
