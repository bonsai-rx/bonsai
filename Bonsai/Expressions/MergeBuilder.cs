using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("Merge", Namespace = Constants.XmlNamespace)]
    [Description("Merges two sequences or a sequence of windows into a single sequence of elements.")]
    public class MergeBuilder : BinaryCombinatorExpressionBuilder
    {
        static readonly MethodInfo binaryMergeMethod = (from method in typeof(Observable).GetMethods()
                                                        where method.Name == "Merge"
                                                        let parameters = method.GetParameters()
                                                        where parameters.Length == 2 &&
                                                              parameters[0].ParameterType.IsGenericType &&
                                                              parameters[1].ParameterType.IsGenericType &&
                                                              parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IObservable<>) &&
                                                              parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(IObservable<>)
                                                        select method)
                                                        .Single();

        static readonly MethodInfo windowMergeMethod = (from method in typeof(Observable).GetMethods()
                                                  where method.Name == "Merge"
                                                  let parameters = method.GetParameters()
                                                  where parameters.Length == 1 && !parameters[0].ParameterType.IsArray &&
                                                        parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IObservable<>) &&
                                                        parameters[0].ParameterType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(IObservable<>)
                                                  select method)
                                                  .Single();

        public override Expression Build()
        {
            var other = Other;
            var observableType = Source.Type.GetGenericArguments()[0];
            if (other != null)
            {
                return Expression.Call(binaryMergeMethod.MakeGenericMethod(observableType), Source, Other);
            }
            else
            {
                var innerType = observableType.GetGenericArguments();
                return Expression.Call(windowMergeMethod.MakeGenericMethod(innerType), Source);
            }
        }
    }
}
