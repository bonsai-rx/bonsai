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
    /// Represents a combinator that returns an observable sequence containing an integer
    /// representing the total number of elements in an observable sequence.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns a sequence containing an integer that represents the total number of elements in the input sequence.")]
    public class Count : Combinator<int>
    {
        /// <summary>
        /// Returns an observable sequence containing an integer representing the
        /// total number of elements in an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">An observable sequence that contains elements to be counted.</param>
        /// <returns>
        /// An observable sequence containing a single element with the number of elements
        /// in the source sequence.
        /// </returns>
        public override IObservable<int> Process<TSource>(IObservable<TSource> source)
        {
            return source.Count();
        }
    }
}
