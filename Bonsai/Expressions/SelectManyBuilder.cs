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
    public class SelectManyBuilder : WorkflowExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(1, 1);
        static readonly MethodInfo returnMethod = (from method in typeof(Observable).GetMethods()
                                                   where method.Name == "Return" && method.GetParameters().Length == 1
                                                   select method)
                                                   .Single();
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

        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }

        public override Expression Build()
        {
            var source = Arguments.Values.Single();
            var sourceType = source.Type.GetGenericArguments()[0];

            // Assign input
            Expression inputParameter;
            var selectorParameter = Expression.Parameter(sourceType);
            if (!sourceType.IsGenericType || sourceType.GetGenericTypeDefinition() != typeof(IObservable<>))
            {
                inputParameter = Expression.Call(returnMethod.MakeGenericMethod(sourceType), selectorParameter);
            }
            else inputParameter = selectorParameter;

            return BuildWorflow(inputParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, selectorParameter);
                var selectorObservableType = selector.ReturnType.GetGenericArguments()[0];
                var selectManyGenericMethod = selectManyMethod.MakeGenericMethod(sourceType, selectorObservableType);
                return Expression.Call(selectManyGenericMethod, source, selector);
            });
        }
    }
}
