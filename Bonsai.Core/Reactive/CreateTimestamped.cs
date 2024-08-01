using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that converts element-timestamp pairs in an observable
    /// sequence into <see cref="Timestamped{T}"/> values.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Converts a sequence of element-timestamp pairs into a sequence of timestamped values.")]
    public class CreateTimestamped
    {
        /// <summary>
        /// Converts element-timestamp pairs in an observable sequence into
        /// <see cref="Timestamped{T}"/> values.
        /// </summary>
        /// <typeparam name="TSource">The type of the value being timestamped.</typeparam>
        /// <param name="source">The sequence of element-timestamp pairs.</param>
        /// <returns>An observable sequence of <see cref="Timestamped{T}"/> values.</returns>
        public IObservable<Timestamped<TSource>> Process<TSource>(IObservable<Tuple<TSource, DateTimeOffset>> source)
        {
            return source.Select(xs => new Timestamped<TSource>(xs.Item1, xs.Item2));
        }
    }
}
