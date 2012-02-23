using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Threading;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml;

namespace Bonsai.IO
{
    public class Timer : Source<Unit>
    {
        System.Threading.Timer timer;

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

        void TimerCallback(object state)
        {
            Subject.OnNext(Unit.Default);
        }

        public override IDisposable Load()
        {
            timer = new System.Threading.Timer(TimerCallback);
            return base.Load();
        }

        protected override void Unload()
        {
            timer.Dispose();
            base.Unload();
        }

        protected override void Start()
        {
            timer.Change(DueTime, Period);
        }

        protected override void Stop()
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }
}
