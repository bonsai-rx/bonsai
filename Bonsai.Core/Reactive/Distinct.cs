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
    /// Represents a combinator that returns an observable sequence containing only distinct elements.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns a sequence that contains only distinct elements.")]
    public class Distinct : Combinator
    {
        /// <summary>
        /// Returns an observable sequence containing only distinct elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An observable sequence to retain distinct elements for.</param>
        /// <returns>
        /// An observable sequence containing only the distinct elements from the source
        /// sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Distinct();
        }
    }
}
