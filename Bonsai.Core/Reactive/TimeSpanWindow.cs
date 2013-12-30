using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;
using System.Reflection;
using System.Reactive.Concurrency;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that projects each element of an observable sequence into consecutive
    /// non-overlapping windows with the specified time interval.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into non-overlapping windows of elements corresponding to the specified time interval.")]
    public class TimeSpanWindow : WindowCombinator
    {
        /// <summary>
        /// Gets or sets the length of each window.
        /// </summary>
        [XmlIgnore]
        [Description("The length of each window.")]
        public TimeSpan TimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of window time span.
        /// </summary>
        [Browsable(false)]
        [XmlElement("TimeSpan")]
        public string TimeSpanXml
        {
            get { return XmlConvert.ToString(TimeSpan); }
            set { TimeSpan = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping
        /// windows with the specified time interval.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to produce windows over.</param>
        /// <returns>An observable sequence of windows.</returns>
        public override IObservable<IObservable<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.Window(TimeSpan, HighResolutionScheduler.Default);
        }
    }
}
