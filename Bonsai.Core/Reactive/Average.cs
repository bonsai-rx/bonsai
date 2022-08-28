using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that computes the numerical average of an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Computes the numerical average of an observable sequence.")]
    public class Average
    {
        /// <summary>
        /// Computes the average of an observable sequence of nullable
        /// <see cref="decimal"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="decimal"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// average of the sequence of values.
        /// </returns>
        public IObservable<decimal?> Process(IObservable<decimal?> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of <see cref="decimal"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="decimal"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// average of the sequence of values.
        /// </returns>
        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of nullable
        /// <see cref="double"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="double"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// average of the sequence of values.
        /// </returns>
        public IObservable<double?> Process(IObservable<double?> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of <see cref="double"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="double"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// average of the sequence of values.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of nullable
        /// <see cref="float"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="float"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// average of the sequence of values.
        /// </returns>
        public IObservable<float?> Process(IObservable<float?> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of <see cref="float"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="float"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// average of the sequence of values.
        /// </returns>
        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of nullable
        /// <see cref="int"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="int"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// average of the sequence of values.
        /// </returns>
        public IObservable<double?> Process(IObservable<int?> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of <see cref="int"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="int"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// average of the sequence of values.
        /// </returns>
        public IObservable<double> Process(IObservable<int> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of nullable
        /// <see cref="long"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="long"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// average of the sequence of values.
        /// </returns>
        public IObservable<double?> Process(IObservable<long?> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of <see cref="long"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="long"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element representing the
        /// average of the sequence of values.
        /// </returns>
        public IObservable<double> Process(IObservable<long> source)
        {
            return source.Average();
        }
    }
}
