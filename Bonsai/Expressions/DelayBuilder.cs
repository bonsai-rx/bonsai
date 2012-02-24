using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("Delay", Namespace = Constants.XmlNamespace)]
    public class DelayBuilder : CombinatorBuilder
    {
        [XmlIgnore]
        public TimeSpan DueTime { get; set; }

        [Browsable(false)]
        [XmlElement("DueTime")]
        public string DueTimeXml
        {
            get { return XmlConvert.ToString(DueTime); }
            set { DueTime = XmlConvert.ToTimeSpan(value); }
        }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Delay(DueTime, HighResolutionScheduler.ThreadPool);
        }
    }
}
