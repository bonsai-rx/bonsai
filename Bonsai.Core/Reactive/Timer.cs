using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an observable sequence that periodically produces a value after the
    /// specified initial relative due time has elapsed.
    /// </summary>
    [DefaultProperty("DueTime")]
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
        /// Gets or sets the XML serializable representation of the due time.
        /// </summary>
        [Browsable(false)]
        [XmlElement("DueTime")]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Gets or sets the XML serializable representation of the period.
        /// </summary>
        [Browsable(false)]
        [XmlElement("Period")]
        public string PeriodXml
        {
            get { return XmlConvert.ToString(Period); }
            set { Period = XmlConvert.ToTimeSpan(value); }
        }

        /// <summary>
        /// Returns an observable sequence that periodically produces a value after the
        /// specified initial relative due time has elapsed.
        /// </summary>
        /// <returns>
        /// An observable sequence that produces a value after due time has elapsed and
        /// then after each period.
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
