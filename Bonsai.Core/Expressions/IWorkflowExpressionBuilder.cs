using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents expression builder instances that generate their output by means
    /// of an encapsulated workflow.
    /// </summary>
    public interface IWorkflowExpressionBuilder : IExpressionBuilder
    {
        /// <summary>
        /// Gets the expression builder workflow that will be used to generate the
        /// output expression tree.
        /// </summary>
        ExpressionBuilderGraph Workflow { get; }
    }
}
