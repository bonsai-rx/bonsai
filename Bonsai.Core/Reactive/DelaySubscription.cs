using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that time-shifts the observable sequence by delaying the
    /// subscription with the specified relative time duration.
    /// </summary>
    [DefaultProperty("DueTime")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Time-shifts the observable sequence by delaying the subscription with the specified relative time duration.")]
    public class DelaySubscription : Combinator
    {
        /// <summary>
        /// Gets or sets the time interval by which to delay the subscription to the sequence.
        /// </summary>
        [XmlIgnore]
        [Description("The time interval by which to delay the subscription to the sequence.")]
        public TimeSpan DueTime { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of due time.
        /// </summary>
        [Browsable(false)]
        [XmlElement("DueTime")]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Time-shifts the observable sequence by delaying the subscription with the specified
        /// relative time duration.
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
