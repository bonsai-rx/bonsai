using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that records the time interval between consecutive
    /// values produced by an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Records the time interval between consecutive elements produced by the sequence.")]
    public class TimeInterval
    {
        /// <summary>
        /// Records the time interval between consecutive values produced by an
        /// observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to record time intervals for.</param>
        /// <returns>
        /// An observable sequence with time interval information for each element.
        /// </returns>
        public IObservable<System.Reactive.TimeInterval<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.TimeInterval(HighResolutionScheduler.Default);
        }
    }
}
