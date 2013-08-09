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
    public abstract class WindowCombinatorExpressionBuilder : WorkflowExpressionBuilder
    {
        static readonly MethodInfo usingMethod = (from method in typeof(Observable).GetMethods()
                                                  where method.Name == "Using"
                                                  let parameters = method.GetParameters()
                                                  let resourceFactoryType = parameters[0].ParameterType
                                                  where resourceFactoryType.GetGenericTypeDefinition() == typeof(Func<>)
                                                  select method)
                                                  .Single();

        protected WindowCombinatorExpressionBuilder()
        {
        }

        protected WindowCombinatorExpressionBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        protected LambdaExpression BuildSourceSelector(Type sourceType)
        {
            // Assign input
            var selectorParameter = Expression.Parameter(sourceType);
            var workflowInput = Workflow.Select(node => node.Value as WorkflowInputBuilder)
                                        .Single(builder => builder != null);
            workflowInput.Source = selectorParameter;

            // Build selector workflow
            var runtimeWorkflow = Workflow.Build();
            var workflowExpression = Expression.Constant(runtimeWorkflow);
            var loadWorkflowExpression = Expression.Call(workflowExpression, "Load", null);
            var resourceFactoryExpression = Expression.Lambda(loadWorkflowExpression);
            var resourceParameter = Expression.Parameter(typeof(IDisposable));

            // Assign output
            var workflowObservableExpression = runtimeWorkflow.Output;

            var workflowObservableType = workflowObservableExpression.Type.GetGenericArguments()[0];
            var observableFactoryExpression = Expression.Lambda(workflowObservableExpression, resourceParameter);
            var usingExpression = Expression.Call(usingMethod.MakeGenericMethod(workflowObservableType, typeof(IDisposable)), resourceFactoryExpression, observableFactoryExpression);
            return Expression.Lambda(usingExpression, selectorParameter);
        }
    }
}
