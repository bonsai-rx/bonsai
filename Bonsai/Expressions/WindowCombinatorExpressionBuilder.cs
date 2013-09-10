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
        static readonly MethodInfo returnMethod = (from method in typeof(Observable).GetMethods()
                                                   where method.Name == "Return" && method.GetParameters().Length == 1
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
            Expression inputParameter;
            var selectorParameter = Expression.Parameter(sourceType);
            if (!sourceType.IsGenericType || sourceType.GetGenericTypeDefinition() != typeof(IObservable<>))
            {
                inputParameter = Expression.Call(returnMethod.MakeGenericMethod(sourceType), selectorParameter);
            }
            else inputParameter = selectorParameter;

            var workflowInput = Workflow.Select(node => node.Value as WorkflowInputBuilder)
                                        .Single(builder => builder != null);
            workflowInput.Source = inputParameter;

            // Build selector workflow
            return Expression.Lambda(Workflow.Build(), selectorParameter);
        }
    }
}
