using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an operator that generates an observable sequence that
    /// periodically produces a value after the specified initial relative
    /// due time has elapsed.
    /// </summary>
    [DefaultProperty(nameof(DueTime))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Generates an observable sequence that periodically produces a value after the specified initial relative due time has elapsed.")]
    public class Timer : Source<long>
    {
        /// <summary>
        /// Gets or sets the relative time at which to produce the first value. If this
        /// value is less than or equal to <see cref="TimeSpan.Zero"/>, the timer will
        /// fire as soon as possible.
        /// </summary>
        [XmlIgnore]
        [Description("The relative time at which to produce the first value. If this value is less than or equal to zero, the timer will fire as soon as possible.")]
        public TimeSpan DueTime { get; set; }

        /// <summary>
        /// Gets or sets the period to produce subsequent values. If this value is equal
        /// to <see cref="TimeSpan.Zero"/> the timer will recur as fast as possible.
        /// </summary>
        [XmlIgnore]
        [Description("The period to produce subsequent values. If this value is equal to zero the timer will recur as fast as possible.")]
        public TimeSpan Period { get; set; }

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
        /// Gets or sets an XML representation of the period for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Period))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string PeriodXml
        {
            get { return XmlConvert.ToString(Period); }
            set { Period = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Generates an observable sequence that periodically produces a value
        /// after the specified initial relative due time has elapsed.
        /// </summary>
        /// <returns>
        /// An observable sequence of integer values counting how many times the
        /// timer has fired.
        /// </returns>
        public override IObservable<long> Generate()
        {
            var period = Period;
            return period > TimeSpan.Zero
                ? Observable.Timer(DueTime, period)
                : Observable.Timer(DueTime);
        }
    }
}
