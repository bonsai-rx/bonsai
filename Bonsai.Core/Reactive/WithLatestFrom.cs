using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that combines the latest values from the source sequences only
    /// when the first sequence produces an element.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Combines the latest values from the source sequences only when the first sequence produces an element.")]
    public class WithLatestFrom
    {
        /// <summary>
        /// Merges the specified sources into one observable sequence by emitting a pair with
        /// the latest source elements only when the first observable sequence produces an
        /// element.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TOther">
        /// The type of the elements in the <paramref name="other"/> sequence.
        /// </typeparam>
        /// <param name="source">The first observable sequence.</param>
        /// <param name="other">The other observable sequence.</param>
        /// <returns>
        /// An observable sequence containing the result of combining the latest elements of the
        /// sources into pairs only when the first sequence produces an element.
        /// </returns>
        public IObservable<Tuple<TSource, TOther>> Process<TSource, TOther>(
            IObservable<TSource> source,
            IObservable<TOther> other)
        {
            return source.Publish(ps =>
                ps.CombineLatest(other, (xs, ys) => Tuple.Create(xs, ys))
                  .Sample(ps)
                  .TakeUntil(ps.IgnoreElements().LastOrDefaultAsync()));
        }
    }
}
