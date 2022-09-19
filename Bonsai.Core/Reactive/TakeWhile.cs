using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder which returns elements from an observable sequence
    /// as long as the condition specified by the encapsulated workflow is true.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Combinator)]
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Returns elements from an observable sequence as long as the condition specified by the encapsulated workflow is true.")]
    public class TakeWhile : Condition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TakeWhile"/> class.
        /// </summary>
        public TakeWhile()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeWhile"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public TakeWhile(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<bool>> condition)
        {
            return Observable.Defer(() =>
            {
                var filter = false;
                return source.Publish(ps => ps
                    .CombineLatest(condition(ps), (xs, ys) => { filter = ys; return xs; })
                    .Sample(ps)
                    .TakeWhile(xs => filter));
            });
        }
    }
}
