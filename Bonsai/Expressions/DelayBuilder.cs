using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Xml.Serialization;
using System.Reflection;
using System.Reactive.Linq;

namespace Bonsai.Expressions
{
    [XmlType("Delay")]
    public class DelayBuilder : CombinatorBuilder
    {
        static readonly MethodInfo delayMethod = typeof(Observable).GetMethods().First(m => m.Name == "Delay" &&
                                                                                       m.GetParameters().Length == 2 &&
                                                                                       m.GetParameters()[1].ParameterType == typeof(TimeSpan));

        public TimeSpan DueTime { get; set; }

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var dueTime = Expression.Constant(DueTime);
            return Expression.Call(delayMethod.MakeGenericMethod(sourceType), Source, dueTime);
        }
    }
}
