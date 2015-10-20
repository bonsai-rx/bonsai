using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a directed acyclic graph of expression generator nodes. Edges between generator nodes
    /// represent input assignments that chain the output of one generator to the input of the next.
    /// The order of the inputs is determined by the indices of the input arguments.
    /// </summary>
    [TypeDescriptionProvider(typeof(ExpressionBuilderTypeDescriptionProvider))]
    public class ExpressionBuilderGraph : DirectedGraph<ExpressionBuilder, ExpressionBuilderArgument>
    {
        class ExpressionBuilderTypeDescriptionProvider : TypeDescriptionProvider
        {
            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                return new WorkflowTypeDescriptor(instance);
            }
        }
    }
}
