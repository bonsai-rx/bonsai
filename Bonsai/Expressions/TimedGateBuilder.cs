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

namespace Bonsai.Expressions
{
    [XmlType("TimedGate", Namespace = Constants.XmlNamespace)]
    public class TimedGateBuilder : BinaryCombinatorBuilder
    {
        [XmlIgnore]
        public TimeSpan TimeSpan { get; set; }

        [Browsable(false)]
        [XmlElement("TimeSpan")]
        public string TimeSpanXml
        {
            get { return XmlConvert.ToString(TimeSpan); }
            set { TimeSpan = XmlConvert.ToTimeSpan(value); }
        }

        protected override IObservable<TSource> Combine<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Gate(other, TimeSpan);
        }
    }
}
