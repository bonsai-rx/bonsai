using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Bonsai.Expressions;
using System.Reactive;
using Rx = System.Reactive.Subjects;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder which repeats an observable sequence until
    /// the condition specified by the encapsulated workflow becomes false.
    /// </summary>
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Repeats the observable sequence until the condition specified by the encapsulated workflow becomes false.")]
    public class RepeatWhile : SingleArgumentWorkflowExpressionBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatWhile"/> class.
        /// </summary>
        public RepeatWhile()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RepeatWhile"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public RepeatWhile(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.Single();
            var sourceType = source.Type.GetGenericArguments()[0];
            var inputParameter = Expression.Parameter(typeof(IObservable<Unit>));
            return BuildWorkflow(arguments.Take(1), inputParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, inputParameter);
                var selectorObservableType = selector.ReturnType.GetGenericArguments()[0];
                if (selectorObservableType != typeof(bool))
                {
                    throw new InvalidOperationException("The specified condition workflow must have a single boolean output.");
                }

                return Expression.Call(
                    typeof(RepeatWhile),
                    nameof(Process),
                    new[] { sourceType },
                    source, selector);
            });
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, Func<IObservable<Unit>, IObservable<bool>> condition)
        {
            return Observable.Using(
                () => new Rx.BehaviorSubject<bool>(false),
                repeat => Observable.Using(
                    () => new Rx.Subject<Unit>(),
                    completed => MergeDependencies(
                        source.DoWhile(() =>
                        {
                            completed.OnNext(Unit.Default);
                            return repeat.Value;
                        }), condition(completed).Do(repeat).IgnoreElements().Select(_ => default(TSource)))));
        }
    }
}
