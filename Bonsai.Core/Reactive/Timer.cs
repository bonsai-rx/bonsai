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
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Returns an observable sequence that periodically produces a value starting at the specified initial relative due time.")]
    public class Timer : Source<long>
    {
        [XmlIgnore]
        public TimeSpan DueTime { get; set; }

        [XmlIgnore]
        public TimeSpan Period { get; set; }

        [Browsable(false)]
        [XmlElement("DueTime")]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        [Browsable(false)]
        [XmlElement("Period")]
        public string PeriodXml
        {
            get { return XmlConvert.ToString(Period); }
            set { Period = XmlConvert.ToTimeSpan(value); }
        }

        public override IObservable<long> Generate()
        {
            var period = Period;
            return period > TimeSpan.Zero
                ? Observable.Timer(DueTime, period)
                : Observable.Timer(DueTime);
        }
    }
}
