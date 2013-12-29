using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Reactive
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Samples values of the sequence each time the specified interval elapses.")]
    public class SampleInterval : Combinator
    {
        [XmlIgnore]
        [Description("The time interval at which to sample the sequence.")]
        public TimeSpan Interval { get; set; }

        [Browsable(false)]
        [XmlElement("Interval")]
        public string IntervalXml
        {
            get { return XmlConvert.ToString(Interval); }
            set { Interval = XmlConvert.ToTimeSpan(value); }
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.Sample(Interval, HighResolutionScheduler.Default);
        }
    }
}
