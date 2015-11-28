using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that projects each element of an observable sequence into zero
    /// or more windows aligned on an external trigger.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into zero or more windows aligned on when the second sequence produces an element.")]
    public class TriggeredWindow
    {
        /// <summary>
        /// Gets or sets the time length of each window. If it is not specified, the window will have
        /// its length set by either a maximum number of elements or an external trigger boundary.
        /// </summary>
        [XmlIgnore]
        [Description("The optional time length of each window.")]
        public TimeSpan? TimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of elements in each window. If it is not specified, the
        /// window will have its length set by the optional time span or by an external trigger boundary.
        /// </summary>
        [Description("The optional maximum number of elements in each window.")]
        public int? Count { get; set; }

        /// <summary>
        /// Gets or sets the XML serializable representation of window time span.
        /// </summary>
        [Browsable(false)]
        [XmlElement("TimeSpan")]
        public string TimeSpanXml
        {
            get
            {
                var timeSpan = TimeSpan;
                if (timeSpan.HasValue) return XmlConvert.ToString(timeSpan.Value);
                else return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value)) TimeSpan = XmlConvert.ToTimeSpan(value);
                else TimeSpan = null;
            }
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more windows.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <typeparam name="TWindowOpening">
        /// The type of the elements in the sequence indicating window opening events.
        /// </typeparam>
        /// <param name="source">The source sequence to produce windows over.</param>
        /// <param name="windowOpenings">
        /// The sequence of window opening events. If no maximum length is specified, the current
        /// window is closed and a new window is opened upon receiving a window opening event.
        /// </param>
        /// <returns>An observable sequence of windows.</returns>
        public IObservable<IObservable<TSource>> Process<TSource, TWindowOpening>(IObservable<TSource> source, IObservable<TWindowOpening> windowOpenings)
        {
            var count = Count;
            var timeSpan = TimeSpan;
            if (timeSpan.HasValue && count.HasValue)
            {
                return source.Publish(ps => ps.Window(windowOpenings, x => Observable.Merge(
                    Observable.Timer(timeSpan.Value, HighResolutionScheduler.Default),
                    ps.Take(count.Value).LongCount())));
            }
            else if (timeSpan.HasValue) return source.Window(windowOpenings, x => Observable.Timer(timeSpan.Value, HighResolutionScheduler.Default));
            else if (count.HasValue) return source.Publish(ps => ps.Window(windowOpenings, x => ps.Take(count.Value).Count()));
            else return source.Window(windowOpenings);
        }
    }
}
