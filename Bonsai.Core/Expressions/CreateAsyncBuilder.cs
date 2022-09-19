using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="CreateAsync"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(CreateAsync))]
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("CreateAsync", Namespace = Constants.XmlNamespace)]
    [Description("Creates and emits the last value of the observable sequence for each subscription using the encapsulated workflow.")]
    public class CreateAsyncBuilder : CreateAsync
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAsyncBuilder"/> class.
        /// </summary>
        public CreateAsyncBuilder()
            : base(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateAsyncBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public CreateAsyncBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }
    }
}
