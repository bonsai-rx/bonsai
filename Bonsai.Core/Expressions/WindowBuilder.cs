using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that projects the input sequence into
    /// zero or more windows with boundaries defined by the encapsulated workflow.
    /// </summary>
    [XmlType("Window", Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into zero or more windows with boundaries defined by the encapsulated workflow.")]
    public class WindowBuilder : WorkflowExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 1, upperBound: 2);
        static readonly MethodInfo returnMethod = (from method in typeof(Observable).GetMethods()
                                                   where method.Name == "Return" && method.GetParameters().Length == 1
                                                   select method)
                                                   .Single();
        static readonly MethodInfo windowOpeningMethod = (from method in typeof(Observable).GetMethods()
                                                          where method.Name == "Window"
                                                          let parameters = method.GetParameters()
                                                          where parameters.Length == 3 &&
                                                                parameters[1].ParameterType.IsGenericType &&
                                                                parameters[1].ParameterType.GetGenericTypeDefinition() == typeof(IObservable<>)
                                                          select method).Single();
        static readonly MethodInfo windowClosingMethod = (from method in typeof(Observable).GetMethods()
                                                          where method.Name == "Window"
                                                          let parameters = method.GetParameters()
                                                          where parameters.Length == 2 && parameters[1].ParameterType.IsGenericType
                                                          let argument = parameters[1].ParameterType.GetGenericArguments()[0]
                                                          where argument.IsGenericType &&
                                                                argument.GetGenericTypeDefinition() == typeof(IObservable<>)
                                                          select method).Single();

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowBuilder"/> class.
        /// </summary>
        public WindowBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public WindowBuilder(ExpressionBuilderGraph workflow)
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
            var sources = arguments.Take(argumentRange.UpperBound).ToArray();
            if (sources.Length == 0)
            {
                throw new InvalidOperationException("There must be at least one input to the Window operator.");
            }

            var source = sources[0];
            Expression inputParameter;
            ParameterExpression selectorParameter;
            var observableType = source.Type.GetGenericArguments()[0];
            if (sources.Length == 1)
            {
                selectorParameter = null;
                inputParameter = Expression.Constant(Observable.Return(Unit.Default), typeof(IObservable<Unit>));
            }
            else
            {
                var openingType = sources[1].Type.GetGenericArguments()[0];
                selectorParameter = Expression.Parameter(openingType);
                inputParameter = Expression.Call(returnMethod.MakeGenericMethod(openingType), selectorParameter);
            }

            return BuildWorkflow(Enumerable.Empty<Expression>(), inputParameter, selectorBody =>
            {
                var closingType = selectorBody.Type.GetGenericArguments()[0];
                if (selectorParameter == null)
                {
                    var factory = Expression.Lambda(selectorBody);
                    return Expression.Call(windowClosingMethod.MakeGenericMethod(observableType, closingType), source, factory);
                }
                else
                {
                    var factory = Expression.Lambda(selectorBody, selectorParameter);
                    return Expression.Call(
                        windowOpeningMethod.MakeGenericMethod(observableType, selectorParameter.Type, closingType),
                        source, sources[1], factory);
                }
            });
        }
    }
}
