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
        static readonly MethodInfo gateMethod = typeof(ObservableCombinators).GetMethods()
                                                                             .First(m => m.Name == "Gate" &&
                                                                                    m.GetParameters().Length == 4);

        [XmlIgnore]
        public TimeSpan TimeSpan { get; set; }

        [Browsable(false)]
        [XmlElement("TimeSpan")]
        public string TimeSpanXml
        {
            get { return XmlConvert.ToString(TimeSpan); }
            set { TimeSpan = XmlConvert.ToTimeSpan(value); }
        }

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var otherType = Other.Type.GetGenericArguments()[0];
            var timeSpan = Expression.Constant(TimeSpan);
            var scheduler = Expression.Constant(HighResolutionScheduler.ThreadPool);
            return Expression.Call(gateMethod.MakeGenericMethod(sourceType, otherType), Source, Other, timeSpan, scheduler);
        }
    }
}
