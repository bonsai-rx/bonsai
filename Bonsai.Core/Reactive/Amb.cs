using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator which propagates the observable sequence that reacts first.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Propagates the observable sequence that reacts first.")]
    public class Amb
    {
        /// <summary>
        /// Propagates the observable sequence that reacts first.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="first">First observable sequence.</param>
        /// <param name="second">Second observable sequence.</param>
        /// <returns>
        /// An observable sequence that surfaces either of the given sequences, whichever
        /// reacted first.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> first, IObservable<TSource> second)
        {
            return first.Amb(second);
        }

        /// <summary>
        /// Propagates the observable sequence that reacts first.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequences.</typeparam>
        /// <param name="sources">Observable sources competing to react first.</param>
        /// <returns>
        /// An observable sequence that surfaces any of the given sequences, whichever
        /// reacted first.
        /// </returns>
        public IObservable<TSource> Process<TSource>(params IObservable<TSource>[] sources)
        {
            return Observable.Amb(sources);
        }
    }
}
