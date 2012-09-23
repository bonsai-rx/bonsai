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
    [Description("Processes each input window using the encapsulated workflow logic.")]
    public class SelectManyBuilder : WorkflowExpressionBuilder
    {
        static readonly MethodInfo usingMethod = typeof(Observable).GetMethod("Using");
        static readonly MethodInfo selectManyMethod = (from method in typeof(Observable).GetMethods()
                                                       where method.Name == "SelectMany"
                                                       let parameters = method.GetParameters()
                                                       where parameters.Length == 2
                                                       let selectorType = parameters[1].ParameterType
                                                       where selectorType.IsGenericType && selectorType.GetGenericTypeDefinition() == typeof(Func<,>) &&
                                                             selectorType.GetGenericArguments()[1].GetGenericTypeDefinition() == typeof(IObservable<>)
                                                       select method)
                                                      .First();

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
            if (!sourceType.IsGenericType || sourceType.GetGenericTypeDefinition() != typeof(IObservable<>))
            {
                throw new InvalidWorkflowException("SelectMany operator takes as input an observable sequence of windows.");
            }

            var selectorParameter = Expression.Parameter(sourceType);
            var workflowInput = Workflow.Select(node => node.Value as WorkflowInputBuilder)
                                        .Single(builder => builder != null);
            workflowInput.Source = selectorParameter;
            var runtimeWorkflow = Workflow.Build();

            var workflowExpression = Expression.Constant(runtimeWorkflow);
            var loadWorkflowExpression = Expression.Call(workflowExpression, "Load", null);
            var resourceFactoryExpression = Expression.Lambda(loadWorkflowExpression);
            var resourceParameter = Expression.Parameter(typeof(IDisposable));
            var workflowObservableExpression = runtimeWorkflow.Connections.Single();
            var workflowObservableType = workflowObservableExpression.Type.GetGenericArguments()[0];
            var observableFactoryExpression = Expression.Lambda(workflowObservableExpression, resourceParameter);
            var usingExpression = Expression.Call(usingMethod.MakeGenericMethod(workflowObservableType, typeof(IDisposable)), resourceFactoryExpression, observableFactoryExpression);

            var selectorExpression = Expression.Lambda(usingExpression, selectorParameter);
            var selectManyGenericMethod = selectManyMethod.MakeGenericMethod(sourceType, workflowObservableType);
            return Expression.Call(selectManyGenericMethod, Source, selectorExpression);
        }
    }
}
