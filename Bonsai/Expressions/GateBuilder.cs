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
    [XmlType("Gate", Namespace = Constants.XmlNamespace)]
    public class GateBuilder : BinaryCombinatorBuilder
    {
        static readonly MethodInfo gateMethod = typeof(ObservableCombinators).GetMethods()
                                                                             .First(m => m.Name == "Gate" &&
                                                                                    m.GetParameters().Length == 2 &&
                                                                                    m.GetParameters()[1].ParameterType.IsGenericType &&
                                                                                    m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(IObservable<>));

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var otherType = Other.Type.GetGenericArguments()[0];
            return Expression.Call(gateMethod.MakeGenericMethod(sourceType, otherType), Source, Other);
        }
    }
}
