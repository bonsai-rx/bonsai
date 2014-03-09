using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Bonsai.Dag
{
    /// <summary>
    /// Represents a labeled node in a directed graph.
    /// </summary>
    /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
    /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
    public class Node<TNodeValue, TEdgeLabel>
    {
        readonly EdgeCollection<TNodeValue, TEdgeLabel> successors = new EdgeCollection<TNodeValue, TEdgeLabel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bonsai.Dag.Node`2{T,U}"/> class with
        /// the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value of the node label.</param>
        public Node(TNodeValue value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value of the <see cref="T:Bonsai.Dag.Node`2{T,U}"/> label.
        /// </summary>
        public TNodeValue Value { get; private set; }

        /// <summary>
        /// Gets the collection of successor edges leaving this node.
        /// </summary>
        public EdgeCollection<TNodeValue, TEdgeLabel> Successors
        {
            get { return successors; }
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="T:Bonsai.Dag.Node`2{T,U}"/> instance.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="T:Bonsai.Dag.Node`2{T,U}"/> object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{{Value = {0}}}", Value);
        }
    }
}
