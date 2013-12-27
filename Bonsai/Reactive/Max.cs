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
    /// Represents a combinator that computes the maximum element in an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Computes the maximum element in an observable sequence.")]
    public class Max
    {
        /// <summary>
        /// Returns the maximum value in an observable sequence of nullable
        /// <see cref="Decimal"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="Decimal"/> values to determine the
        /// maximum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<decimal?> Process(IObservable<decimal?> source)
        {
            return source.Max();
        }

        /// <summary>
        /// Returns the maximum value in an observable sequence of <see cref="Decimal"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Decimal"/> values to determine the maximum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<decimal> Process(IObservable<decimal> source)
        {
            return source.Max();
        }

        /// <summary>
        /// Returns the maximum value in an observable sequence of nullable
        /// <see cref="Double"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="Double"/> values to determine the
        /// maximum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<double?> Process(IObservable<double?> source)
        {
            return source.Max();
        }

        /// <summary>
        /// Returns the maximum value in an observable sequence of <see cref="Double"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Double"/> values to determine the maximum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return source.Max();
        }

        /// <summary>
        /// Returns the maximum value in an observable sequence of nullable
        /// <see cref="Single"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="Single"/> values to determine the
        /// maximum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<float?> Process(IObservable<float?> source)
        {
            return source.Max();
        }

        /// <summary>
        /// Returns the maximum value in an observable sequence of <see cref="Single"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Single"/> values to determine the maximum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<float> Process(IObservable<float> source)
        {
            return source.Max();
        }

        /// <summary>
        /// Returns the maximum value in an observable sequence of nullable
        /// <see cref="Int32"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int32"/> values to determine the
        /// maximum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<int?> Process(IObservable<int?> source)
        {
            return source.Max();
        }

        /// <summary>
        /// Returns the maximum value in an observable sequence of <see cref="Int32"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Int32"/> values to determine the maximum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Max();
        }

        /// <summary>
        /// Returns the maximum value in an observable sequence of nullable
        /// <see cref="Int64"/> values.
        /// </summary>
        /// <param name="source">
        /// A sequence of nullable <see cref="Int64"/> values to determine the
        /// maximum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<long?> Process(IObservable<long?> source)
        {
            return source.Max();
        }

        /// <summary>
        /// Returns the maximum value in an observable sequence of <see cref="Int64"/>
        /// values.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Int64"/> values to determine the maximum value of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<long> Process(IObservable<long> source)
        {
            return source.Max();
        }

        /// <summary>
        /// Returns the maximum element in an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">
        /// An observable sequence to determine the maximum element of.
        /// </param>
        /// <returns>
        /// An observable sequence containing a single element with the maximum value
        /// in the source sequence.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Max();
        }
    }
}
