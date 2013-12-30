using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that returnsthe elements from the source sequence only after the
    /// other sequence produces an element.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns elements from the first sequence only after the second sequence produces an element.")]
    public class SkipUntil : BinaryCombinator
    {
        /// <summary>
        /// Returns the elements from the source observable sequence only after the other
        /// observable sequence produces an element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the other sequence that indicates the end of the skip behavior.
        /// </typeparam>
        /// <param name="source">The source sequence to propagate elements for.</param>
        /// <param name="other">
        /// The observable sequence that triggers propagation of elements of the source sequence.
        /// </param>
        /// <returns>
        /// An observable sequence containing the elements of the source sequence starting
        /// from the point the other sequence triggered propagation.
        /// </returns>
        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.SkipUntil(other);
        }
    }
}
