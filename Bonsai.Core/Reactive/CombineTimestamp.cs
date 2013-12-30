using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that converts element-timestamp tuples of an observable
    /// sequence into proper timestamped elements.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Converts a tuple of element and timestamp into a proper timestamped type.")]
    public class CombineTimestamp
    {
        /// <summary>
        /// Converts element-timestamp tuples of an observable sequence into proper
        /// timestamped elements.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of values in the tuple elements of the source sequence.
        /// </typeparam>
        /// <param name="source">The sequence of element-timestamp tuples.</param>
        /// <returns>An observable sequence of timestamped values.</returns>
        public IObservable<Timestamped<TSource>> Process<TSource>(IObservable<Tuple<TSource, DateTimeOffset>> source)
        {
            return source.Select(xs => new Timestamped<TSource>(xs.Item1, xs.Item2));
        }
    }
}
