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
    [XmlType("Merge", Namespace = Constants.XmlNamespace)]
    public class MergeBuilder : CombinatorExpressionBuilder
    {
        static readonly MethodInfo mergeMethod = typeof(Observable).GetMethods().First(m => m.Name == "Merge" &&
                                                                                       m.GetParameters().Length == 1 &&
                                                                                       !m.GetParameters()[0].ParameterType.IsArray &&
                                                                                       m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IObservable<>));

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var innerType = observableType.GetGenericArguments();
            return Expression.Call(Source, mergeMethod.MakeGenericMethod(innerType));
        }
    }
}
