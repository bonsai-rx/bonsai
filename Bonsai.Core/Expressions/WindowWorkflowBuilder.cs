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
    /// <summary>
    /// Represents an expression builder that generates an expression tree by applying
    /// an encapsulated workflow selector to the elements of an observable sequence of windows.
    /// </summary>
    [XmlType("WindowWorkflow", Namespace = Constants.XmlNamespace)]
    [Description("Processes each input window using the nested workflow.")]
    public class WindowWorkflowBuilder : WorkflowExpressionBuilder
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowWorkflowBuilder"/> class.
        /// </summary>
        public WindowWorkflowBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowWorkflowBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public WindowWorkflowBuilder(ExpressionBuilderGraph workflow)
            : base(workflow, minArguments: 1, maxArguments: 1)
        {
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.FirstOrDefault();
            if (source == null)
            {
                throw new InvalidOperationException("There must be at least one workflow input to WindowWorkflow.");
            }

            var sourceType = source.Type.GetGenericArguments()[0];
            if (!sourceType.IsGenericType || sourceType.GetGenericTypeDefinition() != typeof(IObservable<>))
            {
                throw new InvalidOperationException("WindowWorkflow operator takes as input an observable sequence of windows.");
            }

            var selectorParameter = Expression.Parameter(sourceType);
            return BuildWorflow(arguments, selectorParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, selectorParameter);
                return Expression.Call(selectMethod.MakeGenericMethod(sourceType, selector.ReturnType), source, selector);
            });
        }
    }
}
