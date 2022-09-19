using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="CreateObservable"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(CreateObservable))]
    [XmlType("CreateObservable", Namespace = Constants.XmlNamespace)]
    [Description("Creates higher-order observable sequences using the encapsulated workflow.")]
    public class CreateObservableBuilder : CreateObservable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateObservableBuilder"/> class.
        /// </summary>
        public CreateObservableBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateObservableBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public CreateObservableBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }
    }
}
