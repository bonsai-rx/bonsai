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
    [XmlType("GateInterval", Namespace = Constants.XmlNamespace)]
    public class GateIntervalBuilder : CombinatorBuilder
    {
        static readonly MethodInfo gateMethod = typeof(ObservableCombinators).GetMethods()
                                                                             .First(m => m.Name == "Gate" &&
                                                                                    m.GetParameters().Length == 3 &&
                                                                                    m.GetParameters()[1].ParameterType == typeof(TimeSpan));

        [XmlIgnore]
        public TimeSpan Interval { get; set; }

        [Browsable(false)]
        [XmlElement("Interval")]
        public string IntervalXml
        {
            get { return XmlConvert.ToString(Interval); }
            set { Interval = XmlConvert.ToTimeSpan(value); }
        }

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var interval = Expression.Constant(Interval);
            var scheduler = Expression.Constant(HighResolutionScheduler.ThreadPool);
            return Expression.Call(gateMethod.MakeGenericMethod(observableType), Source, interval, scheduler);
        }
    }
}
