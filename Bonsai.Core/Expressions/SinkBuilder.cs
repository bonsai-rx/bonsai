using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that adds the side effects specified by the
    /// encapsulated workflow to an observable sequence without modifying its elements.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    [XmlType("Sink", Namespace = Constants.XmlNamespace)]
    [Description("Adds side effects specified by the encapsulated workflow to an observable sequence without modifying its elements.")]
    public class SinkBuilder : SingleArgumentWorkflowExpressionBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SinkBuilder"/> class.
        /// </summary>
        public SinkBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SinkBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public SinkBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
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
                throw new InvalidOperationException("There must be at least one workflow input to Sink.");
            }

            // Assign input
            var selectorParameter = Expression.Parameter(source.Type);
            return BuildWorkflow(arguments, selectorParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, selectorParameter);
                var selectorObservableType = selector.ReturnType.GetGenericArguments()[0];
                return Expression.Call(
                    typeof(SinkBuilder), "Process",
                    new [] { source.Type.GetGenericArguments()[0], selectorObservableType },
                    source, selector);
            });
        }

        static IObservable<TSource> Process<TSource, TSink>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TSink>> sink)
        {
            return source.Publish(ps => MergeDependencies(ps, sink(ps).IgnoreElements().Select(xs => default(TSource))));
        }
    }
}
