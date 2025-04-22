using System.ComponentModel;
using System.Xml.Serialization;
using System;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Defer"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Defer))]
    [XmlType("NestedWorkflow", Namespace = Constants.XmlNamespace)]
    [Description("Encapsulates complex workflow logic into a new build context.")]
    public class NestedWorkflowBuilder : Defer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NestedWorkflowBuilder"/> class.
        /// </summary>
        public NestedWorkflowBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedWorkflowBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public NestedWorkflowBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }
    }
}
