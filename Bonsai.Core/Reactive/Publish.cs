using System;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder that shares a single subscription to an observable
    /// sequence across the encapsulated workflow.
    /// </summary>
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Shares a single subscription to an observable sequence across the encapsulated workflow.")]
    public class Publish : MulticastBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Publish"/> class.
        /// </summary>
        public Publish()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Publish"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public Publish(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        internal override IObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        {
            return source.Publish(selector);
        }
    }
}
