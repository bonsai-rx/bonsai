using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a serializable descriptor of the nodes and edges in an expression builder graph.
    /// </summary>
    public class ExpressionBuilderGraphDescriptor : DirectedGraphDescriptor<ExpressionBuilder, ExpressionBuilderArgument>
    {
    }
}
