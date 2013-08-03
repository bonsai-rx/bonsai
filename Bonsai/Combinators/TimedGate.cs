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

namespace Bonsai.Combinators
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Allows a value of the first sequence to propagate if it is produced no more than the specified interval after the second sequence produces an element.")]
    public class TimedGate : BinaryCombinator
    {
        [XmlIgnore]
        [Description("The time interval during which the gate is open after each value of the second sequence is produced.")]
        public TimeSpan TimeSpan { get; set; }

        [Browsable(false)]
        [XmlElement("TimeSpan")]
        public string TimeSpanXml
        {
            get { return XmlConvert.ToString(TimeSpan); }
            set { TimeSpan = XmlConvert.ToTimeSpan(value); }
        }

        public override IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other)
        {
            return source.Gate(other, TimeSpan);
        }
    }
}
