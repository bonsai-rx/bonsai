using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that converts element-timestamp pairs of an observable
    /// sequence into proper timestamped elements.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Converts a pair of element and timestamp into a proper timestamped type.")]
    public class CombineTimestamp
    {
        /// <summary>
        /// Converts element-timestamp pairs of an observable sequence into proper
        /// timestamped elements.
        /// </summary>
        /// <typeparam name="TSource">The type of the value being timestamped.</typeparam>
        /// <param name="source">The sequence of element-timestamp pairs.</param>
        /// <returns>An observable sequence of timestamped values.</returns>
        public IObservable<Timestamped<TSource>> Process<TSource>(IObservable<Tuple<TSource, DateTimeOffset>> source)
        {
            return source.Select(xs => new Timestamped<TSource>(xs.Item1, xs.Item2));
        }
    }
}
