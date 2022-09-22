﻿using System;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="SkipWhile"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(SkipWhile))]
    [WorkflowElementCategory(ElementCategory.Combinator)]
    [XmlType("SkipWhile", Namespace = Constants.XmlNamespace)]
    [Description("Bypasses elements in an observable sequence as long as the condition specified by the encapsulated workflow is true.")]
    public class SkipWhileBuilder : SkipWhile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkipWhileBuilder"/> class.
        /// </summary>
        public SkipWhileBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SkipWhileBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public SkipWhileBuilder(ExpressionBuilderGraph workflow)
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
                    .SkipWhile(xs => filter));
            });
        }
    }
}
