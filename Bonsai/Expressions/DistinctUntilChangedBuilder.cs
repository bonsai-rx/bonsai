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
    [XmlType("DistinctUntilChanged")]
    public class DistinctUntilChangedBuilder : CombinatorBuilder
    {
        static readonly MethodInfo distinctUntilChangedMethod = typeof(Observable).GetMethods().First(m => m.Name == "DistinctUntilChanged" &&
                                                                                                      m.GetParameters().Length == 1);

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            return Expression.Call(distinctUntilChangedMethod.MakeGenericMethod(sourceType), Source);
        }
    }
}
