using System.Collections.Generic;
using Bonsai.Dag;
using System.ComponentModel;
using System;

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
        {
        }

        /// <summary>
        /// Creates and adds a new edge specifying an argument assignment of the source
        /// node to the target node with the specified index.
        /// </summary>
        /// <param name="from">The node that is the source of the edge.</param>
        /// <param name="to">The node that is the target of the edge.</param>
        /// <param name="index">The zero-based index of the input argument.</param>
        /// <returns>The created edge.</returns>
        public Edge<ExpressionBuilder, ExpressionBuilderArgument> AddEdge(
            Node<ExpressionBuilder, ExpressionBuilderArgument> from,
            Node<ExpressionBuilder, ExpressionBuilderArgument> to,
            int index)
        {
            return AddEdge(from, to, new ExpressionBuilderArgument(index));
        }

        /// <summary>
        /// This read-only property is deprecated and should not be used. The getter is
        /// implemented for backward compatibility with legacy clients only.
        /// </summary>
        [Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IComparer<ExpressionBuilder> Comparer
        {
            get { return InstanceComparer; }
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
