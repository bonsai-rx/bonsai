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
    /// Represents a combinator that computes the sum of an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Computes the sum of an observable sequence.")]
    public class Sum
    {
        /// <summary>
        /// Computes the sum of a sequence of nullable <see cref="Decimal"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="Decimal"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the sum of the values
        /// in the source sequence.
        /// </returns>
        public IObservable<decimal?> Process(IObservable<decimal?> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Decimal"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Decimal"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the sum of the values
        /// in the source sequence.
        /// </returns>
        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of nullable <see cref="Double"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="Double"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the sum of the values
        /// in the source sequence.
        /// </returns>
        public IObservable<double?> Process(IObservable<double?> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Double"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Double"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the sum of the values
        /// in the source sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of nullable <see cref="Single"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="Single"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the sum of the values
        /// in the source sequence.
        /// </returns>
        public IObservable<float?> Process(IObservable<float?> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Single"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Single"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the sum of the values
        /// in the source sequence.
        /// </returns>
        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of nullable <see cref="Int32"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int32"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the sum of the values
        /// in the source sequence.
        /// </returns>
        public IObservable<int?> Process(IObservable<int?> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Int32"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Int32"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the sum of the values
        /// in the source sequence.
        /// </returns>
        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of nullable <see cref="Int64"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int64"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the sum of the values
        /// in the source sequence.
        /// </returns>
        public IObservable<long?> Process(IObservable<long?> source)
        {
            return source.Sum();
        }

        /// <summary>
        /// Computes the sum of a sequence of <see cref="Int64"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Int64"/> values to calculate the sum of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the sum of the values
        /// in the source sequence.
        /// </returns>
        public IObservable<long> Process(IObservable<long> source)
        {
            return source.Sum();
        }
    }
}
