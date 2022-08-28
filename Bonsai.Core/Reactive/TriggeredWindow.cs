using System;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that projects each element of an observable sequence into zero
    /// or more windows created when the second sequence emits a notification.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects each element of the sequence into zero or more windows created when the second sequence emits a notification.")]
    public class TriggeredWindow
    {
        /// <summary>
        /// Gets or sets the time length of each window.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the window will have its length specified by either a
        /// maximum number of elements, or the boundary indicated by a notification
        /// from the second sequence.
        /// </remarks>
        [XmlIgnore]
        [Description("The time length of each window.")]
        public TimeSpan? TimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of elements in each window.
        /// </summary>
        /// <remarks>
        /// If no value is specified, the window will have its length specified by either a
        /// maximum time span, or the boundary indicated by a notification from the second
        /// sequence.
        /// </remarks>
        [Description("The maximum number of elements in each window.")]
        public int? Count { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the window time span for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(TimeSpan))]
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
        /// Projects each element of an observable sequence into zero or more windows
        /// created when a second sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <typeparam name="TWindowOpening">
        /// The type of the elements in the <paramref name="windowOpenings"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to produce windows over.</param>
        /// <param name="windowOpenings">
        /// The sequence of window openings. If no maximum length is specified, the current
        /// window is closed and a new window is opened upon receiving a notification from
        /// this sequence.
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
