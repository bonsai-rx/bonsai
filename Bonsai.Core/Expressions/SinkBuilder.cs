using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Reactive.Sink"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Reactive.Sink))]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [XmlType("Sink", Namespace = Constants.XmlNamespace)]
    [Description("Adds side effects specified by the encapsulated workflow to an observable sequence without modifying its elements.")]
    public class SinkBuilder : Reactive.Sink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SinkBuilder"/> class.
        /// </summary>
        public SinkBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SinkBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public SinkBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }
    }
}
