using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("TimeInterval", Namespace = Constants.XmlNamespace)]
    public class TimeIntervalBuilder : CombinatorBuilder
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
