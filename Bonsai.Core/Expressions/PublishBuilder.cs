using System;
using System.Xml.Serialization;
using System.ComponentModel;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Publish"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Publish))]
    [XmlType("Publish", Namespace = Constants.XmlNamespace)]
    [Description("Shares a single subscription to an observable sequence across the encapsulated workflow.")]
    public class PublishBuilder : Publish
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
    }
}
