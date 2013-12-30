using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that samples an observable sequence using a second sequence
    /// producing sampling ticks.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Samples elements of the first sequence only when the second sequence produces a sampling tick.")]
    public class Sample : BinaryCombinator
    {
        /// <summary>
        /// Samples the source observable sequence using a sampler observable sequence
        /// producing sampling ticks. Upon each sampling tick, the latest element (if any)
        /// in the source sequence during the last sampling interval is sent to the
        /// resulting sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TOther">The type of the elements in the sampling sequence.</typeparam>
        /// <param name="source">The source sequence to sample.</param>
        /// <param name="other">The sampling tick sequence.</param>
        /// <returns>The sampled observable sequence.</returns>
        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Sample(other);
        }
    }
}
