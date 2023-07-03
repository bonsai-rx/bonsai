using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that samples the latest element from the sequence
    /// whenever the specified time interval elapses.
    /// </summary>
    [DefaultProperty(nameof(Interval))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Samples the latest element of the sequence whenever the specified time interval elapses.")]
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
        /// Gets or sets an XML representation of the interval for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Interval))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string IntervalXml
        {
            get { return XmlConvert.ToString(Interval); }
            set { Interval = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Samples the latest element from an observable sequence whenever the
        /// specified time interval elapses.
        /// </summary>
        /// <remarks>
        /// Upon each sampling tick, the latest element (if any) emitted by the
        /// <paramref name="source"/> sequence during the last sampling interval
        /// is sent to the resulting sequence.
        /// </remarks>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to sample.</param>
        /// <returns>The sampled observable sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Sample(Interval, HighResolutionScheduler.Default);
        }
    }
}
