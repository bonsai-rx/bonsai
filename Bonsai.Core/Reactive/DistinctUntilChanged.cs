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
    /// Represents a combinator that propagates only distinct contiguous elements
    /// of an observable sequence.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Ensures that only distinct contiguous elements are propagated.")]
    public class DistinctUntilChanged : Combinator
    {
        /// <summary>
        /// Returns an observable sequence that contains only distinct contiguous elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An observable sequence to retain distinct contiguous elements for.</param>
        /// <returns>
        ///  An observable sequence only containing the distinct contiguous elements from
        ///  the source sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.DistinctUntilChanged();
        }
    }
}
