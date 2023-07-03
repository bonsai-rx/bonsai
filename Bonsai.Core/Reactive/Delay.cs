using System;
using System.Xml.Serialization;
using System.Reactive.Linq;
using System.Xml;
using System.ComponentModel;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that delays the notifications of an observable sequence by
    /// the specified time interval.
    /// </summary>
    [DefaultProperty(nameof(DueTime))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Delays the notifications of a sequence by the specified time interval.")]
    public class Delay : Combinator
    {
        /// <summary>
        /// Gets or sets the time interval by which to delay the sequence.
        /// </summary>
        [XmlIgnore]
        [Description("The time interval by which to delay the sequence.")]
        public TimeSpan DueTime { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the due time for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(DueTime))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Delays the notifications of an observable sequence by the specified
        /// time interval.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to delay values for.</param>
        /// <returns>The time-shifted sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Delay(DueTime, HighResolutionScheduler.Default);
        }
    }
}
