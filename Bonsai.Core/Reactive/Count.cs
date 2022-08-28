using System;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that returns the count of the number of elements
    /// in an observable sequence.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns the count of the number of elements in an observable sequence.")]
    public class Count : Combinator<int>
    {
        /// <summary>
        /// Returns the count of the number of elements in an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">An observable sequence that contains elements to be counted.</param>
        /// <returns>
        /// An observable sequence containing a single integer representing the
        /// total number of elements in the <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<int> Process<TSource>(IObservable<TSource> source)
        {
            return source.Count();
        }
    }
}
