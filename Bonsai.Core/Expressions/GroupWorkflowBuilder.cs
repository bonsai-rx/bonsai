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
    /// Represents an expression builder that encapsulates complex expression builder logic into
    /// a single workflow element.
    /// </summary>
    [DisplayName("GroupWorkflow")]
    [WorkflowElementCategory(ElementCategory.Workflow)]
    [XmlType("GroupWorkflow", Namespace = Constants.XmlNamespace)]
    [Description("Encapsulates complex workflow logic into a single workflow element.")]
    public class GroupWorkflowBuilder : WorkflowExpressionBuilder, IGroupWorkflowBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupWorkflowBuilder"/> class.
        /// </summary>
        public GroupWorkflowBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupWorkflowBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public GroupWorkflowBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var groupContext = new GroupContext(BuildContext);
            return Workflow.BuildNested(arguments, groupContext);
        }
    }
}
