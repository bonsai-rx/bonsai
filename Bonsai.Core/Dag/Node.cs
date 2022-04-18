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
        /// Initializes a new instance of the <see cref="Node{T, U}"/> class with
        /// the specified <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value of the node label.</param>
        public Node(TNodeValue value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the value of the node label.
        /// </summary>
        public TNodeValue Value { get; private set; }

        /// <summary>
        /// Gets the collection of successor edges leaving this node.
        /// </summary>
        public EdgeCollection<TNodeValue, TEdgeLabel> Successors
        {
            get { return successors; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{{Value = {Value}}}";
        }
    }
}
