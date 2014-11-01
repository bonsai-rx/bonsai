using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reactive;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that records the timestamp for each element produced by
    /// an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Records the timestamp for each element produced by the sequence.")]
    public class Timestamp
    {
        /// <summary>
        /// Records the timestamp for each element produced by an observable sequence
        /// using a high resolution timer, if available.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to timestamp elements for.</param>
        /// <returns>An observable sequence with timestamp information on elements.</returns>
        public IObservable<Timestamped<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.Timestamp(HighResolutionScheduler.Default);
        }
    }
}
