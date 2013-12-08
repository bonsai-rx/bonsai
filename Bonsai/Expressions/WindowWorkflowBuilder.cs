using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("WindowWorkflow", Namespace = Constants.XmlNamespace)]
    [Description("Processes each input window using the nested workflow.")]
    public class WindowWorkflowBuilder : WorkflowExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(1, 2);
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public WindowWorkflowBuilder()
        {
        }

        public WindowWorkflowBuilder(ExpressionBuilderGraph workflow)
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
            if (!sourceType.IsGenericType || sourceType.GetGenericTypeDefinition() != typeof(IObservable<>))
            {
                throw new InvalidWorkflowException("WindowWorkflow operator takes as input an observable sequence of windows.");
            }

            var selectorParameter = Expression.Parameter(sourceType);
            return BuildWorflow(selectorParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, selectorParameter);
                return Expression.Call(selectMethod.MakeGenericMethod(sourceType, selector.ReturnType), source, selector);
            });
        }
    }
}
