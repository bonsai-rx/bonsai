using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Reflection;

namespace Bonsai.Expressions
{
    [XmlType("Zip", Namespace = Constants.XmlNamespace)]
    public class ZipBuilder : BinaryCombinatorBuilder
    {
        static readonly MethodInfo zipMethod = typeof(Observable).GetMethods().First(m => m.Name == "Zip" &&
                                                                                     m.GetParameters()[1].ParameterType.IsGenericType &&
                                                                                     m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(IObservable<>));
        static readonly MethodInfo tupleCreateMethod = typeof(Tuple).GetMethods().First(m => m.Name == "Create" &&
                                                                                        m.GetParameters().Length == 2);

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var otherType = Other.Type.GetGenericArguments()[0];
            var resultType = typeof(Tuple<,>).MakeGenericType(sourceType, otherType);

            var sourceParameter = Expression.Parameter(sourceType);
            var otherParameter = Expression.Parameter(otherType);
            var selectorBody = Expression.Call(null, tupleCreateMethod.MakeGenericMethod(sourceType, otherType), sourceParameter, otherParameter);
            var selector = Expression.Lambda(selectorBody, sourceParameter, otherParameter);
            return Expression.Call(zipMethod.MakeGenericMethod(sourceType, otherType, resultType), Source, Other, selector);
        }
    }
}
