using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.Reflection;

namespace Bonsai.Expressions
{
    [XmlType("Throttle")]
    public class ThrottleBuilder : CombinatorBuilder
    {
        static readonly MethodInfo throttleMethod = typeof(Observable).GetMethods().First(m => m.Name == "Throttle" &&
                                                                                          m.GetParameters().Length == 2);

        public TimeSpan DueTime { get; set; }

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var dueTime = Expression.Constant(DueTime);
            return Expression.Call(throttleMethod.MakeGenericMethod(sourceType), Source, dueTime);
        }
    }
}
