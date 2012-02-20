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
    [XmlType("Repeat", Namespace = Constants.XmlNamespace)]
    public class RepeatBuilder : CombinatorBuilder
    {
        static readonly MethodInfo repeatMethod = typeof(Observable).GetMethods()
                                                                    .First(m => m.Name == "Repeat" &&
                                                                           m.GetParameters().Length == 1 &&
                                                                           m.GetParameters()[0].ParameterType.IsGenericType &&
                                                                           m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IObservable<>));

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            return Expression.Call(repeatMethod.MakeGenericMethod(observableType), Source);
        }
    }
}
