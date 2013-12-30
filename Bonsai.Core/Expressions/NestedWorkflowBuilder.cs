using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Linq.Expressions;
using Bonsai.Dag;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that generates an expression tree using a nested
    /// expression builder workflow.
    /// </summary>
    [DisplayName("NestedWorkflow")]
    [XmlType("NestedWorkflow", Namespace = Constants.XmlNamespace)]
    [Description("Encapsulates complex workflow logic into a single workflow element.")]
    public class NestedWorkflowBuilder : WorkflowExpressionBuilder
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
            : base(workflow, minArguments: 0, maxArguments: 1)
        {
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node that will be passed on
        /// to other builders in the workflow.
        /// </summary>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build()
        {
            var source = Arguments.SingleOrDefault();
            return BuildWorflow(source, expression => expression);
        }
    }
}
