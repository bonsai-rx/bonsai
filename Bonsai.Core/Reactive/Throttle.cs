using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that ignores elements from an observable sequence which
    /// are followed by another element before the specified duration elapses.
    /// </summary>
    [DefaultProperty(nameof(DueTime))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Ignores elements from the sequence which are followed by another element before the specified duration elapses.")]
    public class Throttle : Combinator
    {
        /// <summary>
        /// Gets or sets the time interval that must elapse before a value is propagated.
        /// </summary>
        [XmlIgnore]
        [Description("The time interval that must elapse before a value is propagated.")]
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
        /// Ignores elements from an observable sequence which are followed by another element
        /// before the specified duration elapses.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to throttle.</param>
        /// <returns>The throttled sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Throttle(DueTime, HighResolutionScheduler.Default);
        }
    }
}
