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
        static readonly ExpressionBuilderComparer InstanceComparer = new ExpressionBuilderComparer();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderGraph"/> class.
        /// </summary>
        public ExpressionBuilderGraph()
            : base(InstanceComparer)
        {
        }

        class ExpressionBuilderComparer : IComparer<ExpressionBuilder>
        {
            public int Compare(ExpressionBuilder x, ExpressionBuilder y)
            {
                if (x == y) return 0;
                else if (x == null) return -1;
                else if (y == null) return 1;
                var comparison = x.InstanceNumber.CompareTo(y.InstanceNumber);
                return comparison != 0 ? comparison : x.DecoratorCounter.CompareTo(y.DecoratorCounter);
            }
        }

        class ExpressionBuilderTypeDescriptionProvider : TypeDescriptionProvider
        {
            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                return new WorkflowTypeDescriptor(instance);
            }
        }
    }
}
