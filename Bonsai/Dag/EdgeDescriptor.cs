using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Dag
{
    /// <summary>
    /// Represents a serializable descriptor of an edge connecting two nodes in a directed graph.
    /// </summary>
    /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
    public class EdgeDescriptor<TEdgeLabel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bonsai.Dag.EdgeDescriptor`1{T}"/> class.
        /// </summary>
        public EdgeDescriptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bonsai.Dag.EdgeDescriptor`1{T}"/> class
        /// with the specified indices for source and target nodes and a label value.
        /// </summary>
        /// <param name="from">The zero-based index of the node that is the source of the edge.</param>
        /// <param name="to">The zero-based index of the node that is the target of the edge.</param>
        /// <param name="label">The value of the edge label.</param>
        public EdgeDescriptor(int from, int to, TEdgeLabel label)
        {
            From = from;
            To = to;
            Label = label;
        }

        /// <summary>
        /// Gets or sets the zero-based index of the node that is the source of the edge.
        /// </summary>
        public int From { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index of the node that is the target of the edge.
        /// </summary>
        public int To { get; set; }

        /// <summary>
        /// Gets or sets the value of the edge label.
        /// </summary>
        public TEdgeLabel Label { get; set; }
    }
}
