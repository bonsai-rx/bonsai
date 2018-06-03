using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that ignores elements from an observable sequence which
    /// are followed by another element before the specified duration elapses.
    /// </summary>
    [DefaultProperty("DueTime")]
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
        /// Gets or sets the XML serializable representation of the throttling duration.
        /// </summary>
        [Browsable(false)]
        [XmlElement("DueTime")]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Ignores elements from an observable sequence which are followed by another element
        /// before the specified duration elapses.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to throttle.</param>
        /// <returns>The throttled sequence.</returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Throttle(DueTime, HighResolutionScheduler.Default);
        }
    }
}
