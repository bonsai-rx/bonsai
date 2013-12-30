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
    /// Represents a combinator that records the time interval between consecutive
    /// values produced by an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Records the time interval between consecutive elements produced by the sequence.")]
    public class TimeInterval
    {
        /// <summary>
        /// Records the time interval between consecutive values produced by an
        /// observable sequence using a high resolution timer, if available.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to record time intervals for.</param>
        /// <returns>An observable sequence with time interval information on elements.</returns>
        public IObservable<System.Reactive.TimeInterval<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.TimeInterval(HighResolutionScheduler.Default);
        }
    }
}
