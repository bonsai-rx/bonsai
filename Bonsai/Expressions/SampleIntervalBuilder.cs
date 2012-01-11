using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("SampleInterval")]
    public class SampleIntervalBuilder : CombinatorBuilder
    {
        static readonly MethodInfo sampleMethod = typeof(Observable).GetMethods()
                                                                    .First(m => m.Name == "Sample" &&
                                                                           m.GetParameters().Length == 2 &&
                                                                           m.GetParameters()[1].ParameterType == typeof(TimeSpan));

        public TimeSpan Interval { get; set; }

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var interval = Expression.Constant(Interval);
            return Expression.Call(sampleMethod.MakeGenericMethod(observableType), Source, interval);
        }
    }
}
