using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that creates a single value observable sequence
    /// from the result of the encapsulated workflow.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("CreateAsync", Namespace = Constants.XmlNamespace)]
    [Description("Creates and emits the last value of the observable sequence for each subscription using the encapsulated workflow.")]
    public class CreateAsyncBuilder : WorkflowExpressionBuilder
    {
        static readonly Expression UnitExpression = Expression.Constant(Observable.Return(Unit.Default), typeof(IObservable<Unit>));
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAsyncBuilder"/> class.
        /// </summary>
        public CreateAsyncBuilder()
            : base(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAsyncBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public CreateAsyncBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
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
            return BuildWorkflow(arguments.Select(x => UnitExpression), null, selectorBody =>
            {
                var factory = Expression.Lambda(selectorBody);
                var resultType = selectorBody.Type.GetGenericArguments()[0];
                if (source != null)
                {
                    var sourceType = source.Type.GetGenericArguments()[0];
                    return Expression.Call(typeof(CreateAsyncBuilder), "Process", new[] { sourceType, resultType }, source, factory);
                }
                else return Expression.Call(typeof(CreateAsyncBuilder), "Process", new[] { resultType }, factory);
            });
        }

        static IObservable<TResult> Process<TResult>(Func<IObservable<TResult>> factory)
        {
            return Observable.Defer(async () => Observable.Return(await factory()));
        }

        static IObservable<TResult> Process<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TResult>> factory)
        {
            return Observable.Defer(async () =>
            {
                var result = await factory();
                return source.Select(x => result);
            });
        }
    }
}
