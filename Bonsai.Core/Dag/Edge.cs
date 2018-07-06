using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Dag
{
    /// <summary>
    /// Provides static methods for creating edge objects.
    /// </summary>
    public static class Edge
    {
        /// <summary>
        /// Creates a new directed graph labeled edge.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="target">The node instance that is the target of the edge.</param>
        /// <param name="label">The value of the edge label.</param>
        /// <returns>A labeled edge targeting the specified node.</returns>
        public static Edge<TNodeValue, TEdgeLabel> Create<TNodeValue, TEdgeLabel>(Node<TNodeValue, TEdgeLabel> target, TEdgeLabel label)
        {
            return new Edge<TNodeValue, TEdgeLabel>(target, label);
        }
    }

    /// <summary>
    /// Represents an outgoing labeled edge in a directed graph.
    /// </summary>
    /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
    /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
    public class Edge<TNodeValue, TEdgeLabel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Bonsai.Dag.Edge`2{T,U}"/> class with
        /// the specified <paramref name="target"/> node and <paramref name="label"/>.
        /// </summary>
        /// <param name="target">
        /// The <see cref="T:Bonsai.Dag.Node`2{T,U}"/> instance that is the target
        /// of the edge.
        /// </param>
        /// <param name="label">The value of the edge label.</param>
        public Edge(Node<TNodeValue, TEdgeLabel> target, TEdgeLabel label)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

            Label = label;
            Target = target;
        }

        /// <summary>
        /// Gets the value of the <see cref="T:Bonsai.Dag.Edge`2{T,U}"/> label.
        /// </summary>
        public TEdgeLabel Label { get; private set; }

        /// <summary>
        /// Gets the <see cref="T:Bonsai.Dag.Node`2{T,U}"/> instance that is the
        /// target of the <see cref="T:Bonsai.Dag.Edge`2{T,U}"/>.
        /// </summary>
        public Node<TNodeValue, TEdgeLabel> Target { get; private set; }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// <b>true</b> if <paramref name="obj"/> is an instance of <see cref="Edge{TNodeValue, TEdgeLabel}"/>
        /// and its index equals the index value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            var edge = obj as Edge<TNodeValue, TEdgeLabel>;
            if (edge == null) return false;
            return Target == edge.Target && EqualityComparer<TEdgeLabel>.Default.Equals(Label, edge.Label);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Target.GetHashCode() ^ EqualityComparer<TEdgeLabel>.Default.GetHashCode(Label);
        }

        /// <summary>
        /// Returns a string that represents this <see cref="T:Bonsai.Dag.Edge`2{T,U}"/> instance.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="T:Bonsai.Dag.Edge`2{T,U}"/> object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{{Label = {0}, Target = {1}}}", Label, Target);
        }
    }
}
