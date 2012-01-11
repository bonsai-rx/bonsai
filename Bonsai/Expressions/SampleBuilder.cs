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
    [XmlType("Sample")]
    public class SampleBuilder : BinaryCombinatorBuilder
    {
        static readonly MethodInfo sampleMethod = typeof(Observable).GetMethods()
                                                                    .First(m => m.Name == "Sample" &&
                                                                           m.GetParameters().Length == 2 &&
                                                                           m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(IObservable<>));

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var otherType = Other.Type.GetGenericArguments()[0];
            return Expression.Call(sampleMethod.MakeGenericMethod(sourceType, otherType), Source, Other);
        }
    }
}
