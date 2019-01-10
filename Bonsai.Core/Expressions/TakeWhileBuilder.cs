using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder which returns elements from an observable sequence
    /// as long as the condition specified by the encapsulated workflow is true.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Combinator)]
    [XmlType("TakeWhile", Namespace = Constants.XmlNamespace)]
    [Description("Returns elements from an observable sequence as long as the condition specified by the encapsulated workflow is true.")]
    public class TakeWhileBuilder : ConditionBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TakeWhileBuilder"/> class.
        /// </summary>
        public TakeWhileBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TakeWhileBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public TakeWhileBuilder(ExpressionBuilderGraph workflow)
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
