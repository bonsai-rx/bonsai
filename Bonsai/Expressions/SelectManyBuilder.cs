using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Linq.Expressions;
using Bonsai.Dag;
using System.Reactive.Linq;
using System.Reflection;

namespace Bonsai.Expressions
{
    [XmlType("SelectMany", Namespace = Constants.XmlNamespace)]
    [Description("Processes each input window using the nested workflow and merges the result into a single sequence.")]
    public class SelectManyBuilder : WindowCombinatorExpressionBuilder
    {
        static readonly MethodInfo selectManyMethod = (from method in typeof(Observable).GetMethods()
                                                       where method.Name == "SelectMany"
                                                       let parameters = method.GetParameters()
                                                       where parameters.Length == 2
                                                       let selectorType = parameters[1].ParameterType
                                                       where selectorType.IsGenericType && selectorType.GetGenericTypeDefinition() == typeof(Func<,>) &&
                                                             selectorType.GetGenericArguments()[1].GetGenericTypeDefinition() == typeof(IObservable<>)
                                                       select method)
                                                      .Single();

        public SelectManyBuilder()
        {
        }

        public SelectManyBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var selectorExpression = BuildSourceSelector(sourceType);
            var selectorObservableType = selectorExpression.ReturnType.GetGenericArguments()[0];
            var selectManyGenericMethod = selectManyMethod.MakeGenericMethod(sourceType, selectorObservableType);
            return Expression.Call(selectManyGenericMethod, Source, selectorExpression);
        }
    }
}
