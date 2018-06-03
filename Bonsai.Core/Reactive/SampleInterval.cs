using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that samples the observable sequence at each interval. Upon each
    /// sampling tick, the latest element (if any) in the source sequence during the last sampling
    /// interval is sent to the resulting sequence.
    /// </summary>
    [DefaultProperty("Interval")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Samples the latest element of the sequence each time the specified interval elapses.")]
    public class SampleInterval : Combinator
    {
        /// <summary>
        /// Gets or sets the interval at which to sample. If this value is equal to
        /// <see cref="TimeSpan.Zero"/>, the scheduler will continuously sample the stream.
        /// </summary>
        [XmlIgnore]
        [Description("The time interval at which to sample the sequence.")]
        public TimeSpan Interval { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of the interval.
        /// </summary>
        [Browsable(false)]
        [XmlElement("Interval")]
        public string IntervalXml
        {
            get { return XmlConvert.ToString(Interval); }
            set { Interval = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Samples the observable sequence at each interval.  Upon each sampling tick,
        /// the latest element (if any) in the source sequence during the last sampling
        /// interval is sent to the resulting sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to sample.</param>
        /// <returns>The sampled observable sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Sample(Interval, HighResolutionScheduler.Default);
        }
    }
}
