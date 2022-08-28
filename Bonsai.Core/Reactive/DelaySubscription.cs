using System;
using System.Xml.Serialization;
using System.Reactive.Linq;
using System.Xml;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that time-shifts the observable sequence by delaying the
    /// subscription by the specified time interval.
    /// </summary>
    [DefaultProperty(nameof(DueTime))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Time-shifts the observable sequence by delaying the subscription by the specified time interval.")]
    public class DelaySubscription : Combinator
    {
        /// <summary>
        /// Gets or sets the time interval by which to delay the subscription to the sequence.
        /// </summary>
        [XmlIgnore]
        [Description("The time interval by which to delay the subscription to the sequence.")]
        public TimeSpan DueTime { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the due time for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(DueTime))]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Time-shifts the observable sequence by delaying the subscription by the specified
        /// time interval.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to delay subscription for.</param>
        /// <returns>The time-shifted sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.DelaySubscription(DueTime, HighResolutionScheduler.Default);
        }
    }
}
