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
    [Description("Merges the sequence of windows into a single sequence of elements.")]
    public class MergeBuilder : CombinatorExpressionBuilder
    {
        static readonly MethodInfo mergeMethod = (from method in typeof(Observable).GetMethods()
                                                  where method.Name == "Merge"
                                                  let parameters = method.GetParameters()
                                                  where parameters.Length == 1 && !parameters[0].ParameterType.IsArray &&
                                                        parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IObservable<>) &&
                                                        parameters[0].ParameterType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(IObservable<>)
                                                  select method)
                                                  .Single();

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var innerType = observableType.GetGenericArguments();
            return Expression.Call(mergeMethod.MakeGenericMethod(innerType), Source);
        }
    }
}
