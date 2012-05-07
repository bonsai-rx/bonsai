using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Xml;
using System.Reflection;
using System.Reactive.Concurrency;

namespace Bonsai.Expressions
{
    [XmlType("TimeSpanWindow", Namespace = Constants.XmlNamespace)]
    public class TimeSpanWindowBuilder : WindowBuilder
    {
        [XmlIgnore]
        public TimeSpan Length { get; set; }

        [Browsable(false)]
        [XmlElement("Length")]
        public string LengthXml
        {
            get { return XmlConvert.ToString(Length); }
            set { Length = XmlConvert.ToTimeSpan(value); }
        }

        protected override IObservable<IObservable<TSource>> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Window(Length, HighResolutionScheduler.ThreadPool);
        }
    }
}
