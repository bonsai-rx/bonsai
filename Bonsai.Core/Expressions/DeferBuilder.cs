using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Defer"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Defer))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("Defer", Namespace = Constants.XmlNamespace)]
    [Description("Creates a new observable sequence for each subscription using the encapsulated workflow.")]
    public class DeferBuilder : Defer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeferBuilder"/> class.
        /// </summary>
        public DeferBuilder()
            : base(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public DeferBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }
    }
}
