﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder which accumulates the values of an observable
    /// sequence using the encapsulated workflow.
    /// </summary>
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Accumulates the values of an observable sequence using the encapsulated workflow.")]
    public class Scan : WorkflowExpressionBuilder
    {
        static readonly Range<int> argumentRange = Bonsai.Range.Create(lowerBound: 1, upperBound: 2);

        /// <summary>
        /// Initializes a new instance of the <see cref="Scan"/> class.
        /// </summary>
        public Scan()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scan"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public Scan(ExpressionBuilderGraph workflow)
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
                throw new InvalidOperationException("There must be at least one workflow input to Scan.");
            }

            // Assign accumulation seed
            Type seedType;
            Expression seed;
            var source = sources[0];
            var sourceType = source.Type.GetGenericArguments()[0];
            if (sources.Length > argumentRange.LowerBound)
            {
                seed = sources[sources.Length - 1];
                seedType = seed.Type.GetGenericArguments()[0];
            }
            else
            {
                seed = null;
                seedType = sourceType;
            }

            var memoryType = typeof(ElementAccumulation<,>).MakeGenericType(seedType, sourceType);
            var inputParameter = Expression.Parameter(typeof(IObservable<>).MakeGenericType(memoryType));
            return BuildWorkflow(arguments.Take(1), inputParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, inputParameter);
                var selectorObservableType = selector.ReturnType.GetGenericArguments()[0];
                if (selectorObservableType != seedType)
                {
                    throw new InvalidOperationException("The specified scan workflow must have a single output of the same type as the accumulation.");
                }

                if (seed == null)
                {
                    return Expression.Call(
                        typeof(Scan),
                        nameof(Process),
                        new[] { sourceType },
                        source, selector);
                }
                else
                {
                    return Expression.Call(
                        typeof(Scan),
                        nameof(Process),
                        new[] { sourceType, seedType },
                        source,
                        seed,
                        selector);
                }
            });
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, Func<IObservable<ElementAccumulation<TSource, TSource>>, IObservable<TSource>> selector)
        {
            return Observable.Defer(() =>
            {
                var gate = new object();
                var memory = default(TSource);
                var feed = source.Publish(ps =>
                    ps.Take(1).Concat(selector(
                    ps.Select(input =>
                    {
                        lock (gate)
                        {
                            return new ElementAccumulation<TSource, TSource>(memory, input);
                        }
                    }))));
                return feed.Do(result =>
                {
                    lock (gate)
                    {
                        memory = result;
                    }
                });
            });
        }

        static IObservable<TSeed> Process<TSource, TSeed>(
            IObservable<TSource> source,
            IObservable<TSeed> seed,
            Func<IObservable<ElementAccumulation<TSeed, TSource>>, IObservable<TSeed>> selector)
        {
            return Observable.Defer(() =>
            {
                var gate = new object();
                return seed.Take(1).SelectMany(seed =>
                {
                    var memory = seed;
                    var feed = source.Select(input =>
                    {
                        lock (gate)
                        {
                            return new ElementAccumulation<TSeed, TSource>(memory, input);
                        }
                    });
                    return selector(feed).Do(result =>
                    {
                        lock (gate)
                        {
                            memory = result;
                        }
                    });
                });
            });
        }
    }
}
