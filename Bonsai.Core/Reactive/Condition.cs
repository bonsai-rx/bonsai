﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder which filters the elements of an observable
    /// sequence according to a condition specified by the encapsulated workflow.
    /// </summary>
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Filters the elements of an observable sequence according to a condition specified by the encapsulated workflow.")]
    public class Condition : SingleArgumentWorkflowExpressionBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Condition"/> class.
        /// </summary>
        public Condition()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Condition"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public Condition(ExpressionBuilderGraph workflow)
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
                throw new InvalidOperationException("There must be at least one input to the condition workflow.");
            }

            // Assign input
            var selectorParameter = Expression.Parameter(source.Type);
            return BuildWorkflow(arguments, selectorParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, selectorParameter);
                var selectorObservableType = selector.ReturnType.GetGenericArguments()[0];
                if (selectorObservableType != typeof(bool))
                {
                    throw new InvalidOperationException("The specified condition workflow must have a single boolean output.");
                }

                return Expression.Call(GetType(), nameof(Process), source.Type.GetGenericArguments(), source, selector);
            });
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<bool>> condition)
        {
            return Observable.Defer(() =>
            {
                var filter = false;
                return source.Publish(ps => ps
                    .CombineLatest(condition(ps), (xs, ys) => { filter = ys; return xs; })
                    .Sample(ps)
                    .Where(xs => filter));
            });
        }
    }
}
