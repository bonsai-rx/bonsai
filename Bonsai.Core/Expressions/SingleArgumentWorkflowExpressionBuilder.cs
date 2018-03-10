using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for workflow expression builders that require a single input argument.
    /// This is an abstract class.
    /// </summary>
    public abstract class SingleArgumentWorkflowExpressionBuilder : WorkflowExpressionBuilder
    {
        internal SingleArgumentWorkflowExpressionBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        internal SingleArgumentWorkflowExpressionBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get
            {
                var parameterCount = Workflow.GetNestedParameters().Count();
                return Range.Create(Math.Max(1, parameterCount), Math.Max(1, parameterCount));
            }
        }
    }
}
