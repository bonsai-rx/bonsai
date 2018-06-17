using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that generates a sequence of <see cref="Unit"/> elements.
    /// </summary>
    /// <remarks>
    /// This expression builder generates its elements by either returning the single default
    /// <see cref="Unit"/> instance if no input sequence is provided; or applying a selector
    /// on the elements of the source sequence that will convert each input element into the
    /// default <see cref="Unit"/> instance.
    /// </remarks>
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("Unit", Namespace = Constants.XmlNamespace)]
    [Description("Generates a sequence of Unit type elements.")]
    public class UnitBuilder : ExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 1);

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
            var source = arguments.SingleOrDefault();
            if (source == null)
            {
                return Expression.Constant(Observable.Return(Unit.Default), typeof(IObservable<Unit>));
            }
            else return Expression.Call(typeof(UnitBuilder), "Process", source.Type.GetGenericArguments(), source);
        }

        static IObservable<Unit> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(xs => Unit.Default);
        }
    }
}
