using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Threading;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml;
using System.Reactive.Linq;

namespace Bonsai.IO
{
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
            return Observable.Timer(DueTime, Period);
        }
    }
}
