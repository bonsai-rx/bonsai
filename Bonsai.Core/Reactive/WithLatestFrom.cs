using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that combines the latest values from the source sequences only
    /// when the first sequence produces an element.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Combines the latest values from the source sequences only when the first sequence produces an element.")]
    public class WithLatestFrom
    {
        /// <summary>
        /// Merges the specified sources into one observable sequence by emitting a tuple with
        /// the latest source elements only when the first observable sequence produces an
        /// element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the first source sequence.</typeparam>
        /// <typeparam name="TOther">The type of the elements in the other source sequence.</typeparam>
        /// <param name="source">The first observable sequence.</param>
        /// <param name="other">The other observable sequence.</param>
        /// <returns>
        /// An observable sequence containing the result of combining the latest elements of the
        /// sources into tuples only when the first sequence produces an element.
        /// </returns>
        public IObservable<Tuple<TSource, TOther>> Process<TSource, TOther>(
            IObservable<TSource> source,
            IObservable<TOther> other)
        {
            return source.Publish(ps =>
                ps.CombineLatest(other, (xs, ys) => Tuple.Create(xs, ys))
                  .Sample(ps));
        }
    }
}
