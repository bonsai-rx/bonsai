using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that creates a new observable sequence
    /// for each subscription using the encapsulated workflow.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("Defer", Namespace = Constants.XmlNamespace)]
    [Description("Creates a new observable sequence for each subscription using the encapsulated workflow.")]
    public class DeferBuilder : NestedWorkflowBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 0);

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

        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
        }
    }
}
