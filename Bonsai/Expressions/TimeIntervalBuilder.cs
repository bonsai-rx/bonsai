using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("TimeInterval", Namespace = Constants.XmlNamespace)]
    [Description("Records the time interval between consecutive values produced by the sequence.")]
    public class TimeIntervalBuilder : CombinatorExpressionBuilder
    {
        static readonly MethodInfo timeIntervalMethod = typeof(Observable).GetMethods()
                                                                          .First(m => m.Name == "TimeInterval" &&
                                                                                 m.GetParameters().Length == 2);

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var scheduler = Expression.Constant(HighResolutionScheduler.ThreadPool);
            return Expression.Call(timeIntervalMethod.MakeGenericMethod(observableType), Source, scheduler);
        }
    }
}
