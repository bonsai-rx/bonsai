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
    [XmlType("SampleInterval", Namespace = Constants.XmlNamespace)]
    public class SampleIntervalBuilder : CombinatorBuilder
    {
        static readonly MethodInfo sampleMethod = typeof(Observable).GetMethods()
                                                                    .First(m => m.Name == "Sample" &&
                                                                           m.GetParameters().Length == 2 &&
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
            return Expression.Call(sampleMethod.MakeGenericMethod(observableType), Source, interval);
        }
    }
}
