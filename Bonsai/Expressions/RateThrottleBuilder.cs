using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.Xml;

namespace Bonsai.Expressions
{
    [XmlType("RateThrottle", Namespace = Constants.XmlNamespace)]
    public class RateThrottleBuilder : CombinatorBuilder
    {
        static readonly MethodInfo rateThrottleMethod = typeof(ObservableCombinators).GetMethods().First(m => m.Name == "RateThrottle" &&
                                                                                                        m.GetParameters().Length == 2);

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
            return Expression.Call(rateThrottleMethod.MakeGenericMethod(observableType), Source, interval);
        }
    }
}
