using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that shares a single subscription to an observable
    /// sequence across the encapsulated workflow.
    /// </summary>
    [XmlType("Publish", Namespace = Constants.XmlNamespace)]
    [Description("Shares a single subscription to an observable sequence across the encapsulated workflow.")]
    public class PublishBuilder : MulticastBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublishBuilder"/> class.
        /// </summary>
        public PublishBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public PublishBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        internal override IObservable<TResult> Multicast<TSource, TResult>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        {
            return source.Publish(selector);
        }
    }
}
