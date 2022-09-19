using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that projects each element of an observable sequence into zero
    /// or more windows based on timing information.
    /// </summary>
    [DefaultProperty(nameof(TimeSpan))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into zero or more windows based on timing information.")]
    public class WindowTime : WindowCombinator
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
        /// <remarks>
        /// If no value is specified, the operator will generate consecutive
        /// non-overlapping windows.
        /// </remarks>
        [XmlIgnore]
        [Description("The interval between creation of consecutive windows.")]
        public TimeSpan? TimeShift { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the window time span for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(TimeSpan))]
        public string TimeSpanXml
        {
            get { return XmlConvert.ToString(TimeSpan); }
            set { TimeSpan = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Gets or sets an XML representation of the window time shift for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(TimeShift))]
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
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">The source sequence to produce windows over.</param>
        /// <returns>An observable sequence of windows.</returns>
        public override IObservable<IObservable<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            var timeShift = TimeShift;
            if (timeShift.HasValue) return source.Window(TimeSpan, timeShift.Value, HighResolutionScheduler.Default);
            else return source.Window(TimeSpan, HighResolutionScheduler.Default);
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="WindowTime"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(WindowTime))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class TimeSpanWindow : WindowTime
    {
    }
}
