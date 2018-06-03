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
    /// Represents a combinator that projects each element of an observable sequence into zero
    /// or more windows based on timing information.
    /// </summary>
    [DefaultProperty("TimeSpan")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into zero or more windows based on timing information.")]
    public class TimeSpanWindow : WindowCombinator
    {
        /// <summary>
        /// Gets or sets the length of each window.
        /// </summary>
        [XmlIgnore]
        [Description("The length of each window.")]
        public TimeSpan TimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the interval between creation of consecutive windows.
        /// </summary>
        [XmlIgnore]
        [Description("The optional interval between creation of consecutive windows.")]
        public TimeSpan? TimeShift { get; set; }

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
        /// Gets or sets the XML serializable representation of window interval.
        /// </summary>
        [Browsable(false)]
        [XmlElement("TimeShift")]
        public string TimeShiftXml
        {
            get
            {
                var timeShift = TimeShift;
                if (timeShift.HasValue) return XmlConvert.ToString(timeShift.Value);
                else return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value)) TimeShift = XmlConvert.ToTimeSpan(value);
                else TimeShift = null;
            }
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more windows
        /// based on timing information.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence to produce windows over.</param>
        /// <returns>An observable sequence of windows.</returns>
        public override IObservable<IObservable<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            var timeShift = TimeShift;
            if (timeShift.HasValue) return source.Window(TimeSpan, timeShift.Value, HighResolutionScheduler.Default);
            else return source.Window(TimeSpan, HighResolutionScheduler.Default);
        }
    }
}
