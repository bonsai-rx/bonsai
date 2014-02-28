using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that computes the numerical average of an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Computes the numerical average of an observable sequence.")]
    public class Average
    {
        /// <summary>
        /// Computes the average of an observable sequence of nullable <see cref="Decimal"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Decimal"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the average of the
        /// sequence of values.
        /// </returns>
        public IObservable<decimal?> Process(IObservable<decimal?> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of <see cref="Decimal"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Decimal"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the average of the
        /// sequence of values.
        /// </returns>
        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of nullable <see cref="Double"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Double"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the average of the
        /// sequence of values.
        /// </returns>
        public IObservable<double?> Process(IObservable<double?> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of <see cref="Double"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Double"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the average of the
        /// sequence of values.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of nullable <see cref="Single"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Single"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the average of the
        /// sequence of values.
        /// </returns>
        public IObservable<float?> Process(IObservable<float?> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of <see cref="Single"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Single"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the average of the
        /// sequence of values.
        /// </returns>
        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of nullable <see cref="Int32"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Int32"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the average of the
        /// sequence of values.
        /// </returns>
        public IObservable<double?> Process(IObservable<int?> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of <see cref="Int32"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Int32"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the average of the
        /// sequence of values.
        /// </returns>
        public IObservable<double> Process(IObservable<int> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of nullable <see cref="Int64"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Int64"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the average of the
        /// sequence of values.
        /// </returns>
        public IObservable<double?> Process(IObservable<long?> source)
        {
            return source.Average();
        }

        /// <summary>
        /// Computes the average of an observable sequence of <see cref="Int64"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Int64"/> values to calculate the average of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the average of the
        /// sequence of values.
        /// </returns>
        public IObservable<double> Process(IObservable<long> source)
        {
            return source.Average();
        }
    }
}
