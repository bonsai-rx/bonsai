using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Defines methods that support the generation of expression tree nodes from
    /// a collection of expression input arguments.
    /// </summary>
    public interface IExpressionBuilder
    {
        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        Range<int> ArgumentRange { get; }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        Expression Build(IEnumerable<Expression> arguments);
    }
}
