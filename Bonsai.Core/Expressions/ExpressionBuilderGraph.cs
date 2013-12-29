using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a directed acyclic graph of expression generator nodes. Edges between generator nodes
    /// represent input assignments that chain the output of one generator to the input of the next.
    /// The order of the inputs is determined by the indices of the input arguments.
    /// </summary>
    public class ExpressionBuilderGraph : DirectedGraph<ExpressionBuilder, ExpressionBuilderArgument>
    {
    }
}
