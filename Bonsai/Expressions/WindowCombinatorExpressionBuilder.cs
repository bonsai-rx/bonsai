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
            return Expression.Lambda(Workflow.Build(), selectorParameter);
        }
    }
}
