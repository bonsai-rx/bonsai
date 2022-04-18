using System;
using System.Collections.Generic;

namespace Bonsai.Dag
{
    /// <summary>
    /// Represents a directed graph that consists of labeled nodes and edges.
    /// </summary>
    /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
    /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
    public class DirectedGraph<TNodeValue, TEdgeLabel> : IEnumerable<Node<TNodeValue, TEdgeLabel>>
    {
        readonly ISet<Node<TNodeValue, TEdgeLabel>> nodes;
        readonly IComparer<TNodeValue> valueComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{T, U}"/> class.
        /// </summary>
        public DirectedGraph()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{T, U}"/> class
        /// that uses a specified comparer.
        /// </summary>
        /// <param name="comparer">The optional comparer to use for ordering node values.</param>
        public DirectedGraph(IComparer<TNodeValue> comparer)
        {
            if (comparer == null)
            {
                nodes = new HashSet<Node<TNodeValue, TEdgeLabel>>();
            }
            else
            {
                var nodeComparer = new NodeComparer(comparer);
                nodes = new SortedSet<Node<TNodeValue, TEdgeLabel>>(nodeComparer);
                valueComparer = comparer;
            }
        }

        /// <summary>
        /// Gets the optional <see cref="IComparer{TNodeValue}"/> object used to determine
        /// the order of the values in the directed graph.
        /// </summary>
        public IComparer<TNodeValue> Comparer
        {
            get { return valueComparer; }
        }

        class NodeComparer : IComparer<Node<TNodeValue, TEdgeLabel>>
        {
            readonly IComparer<TNodeValue> valueComparer;

            internal NodeComparer(IComparer<TNodeValue> comparer)
            {
                valueComparer = comparer;
            }

            public int Compare(Node<TNodeValue, TEdgeLabel> x, Node<TNodeValue, TEdgeLabel> y)
            {
                return valueComparer.Compare(x.Value, y.Value);
            }
        }

        /// <summary>
        /// Gets the number of nodes in the directed graph.
        /// </summary>
        public int Count
        {
            get { return nodes.Count; }
        }

        /// <summary>
        /// Creates and adds a new node with the specified value to the
        /// directed graph.
        /// </summary>
        /// <param name="value">The value of the node label.</param>
        /// <returns>The created node.</returns>
        public Node<TNodeValue, TEdgeLabel> Add(TNodeValue value)
        {
            var node = new Node<TNodeValue, TEdgeLabel>(value);
            Add(node);
            return node;
        }

        /// <summary>
        /// Adds a node to the directed graph.
        /// </summary>
        /// <param name="node">The node to be added to the directed graph.</param>
        public void Add(Node<TNodeValue, TEdgeLabel> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            nodes.Add(node);
            foreach (var successor in node.Successors)
            {
                if (nodes.Contains(successor.Target)) continue;
                Add(successor.Target);
            }
        }

        /// <summary>
        /// Creates and adds a new labeled edge linking the specified nodes in the
        /// directed graph.
        /// </summary>
        /// <param name="from">The node that is the source of the edge.</param>
        /// <param name="to">The node that is the target of the edge.</param>
        /// <param name="label">The value of the edge label.</param>
        /// <returns>The created edge.</returns>
        public Edge<TNodeValue, TEdgeLabel> AddEdge(Node<TNodeValue, TEdgeLabel> from, Node<TNodeValue, TEdgeLabel> to, TEdgeLabel label)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            if (!nodes.Contains(from))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", nameof(from));
            }

            if (!nodes.Contains(to))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", nameof(to));
            }

            var edge = new Edge<TNodeValue, TEdgeLabel>(to, label);
            from.Successors.Add(edge);
            return edge;
        }

        /// <summary>
        /// Adds a labeled outgoing edge from the specified node in the
        /// directed graph.
        /// </summary>
        /// <param name="from">The node that is the source of the edge.</param>
        /// <param name="edge">
        /// The labeled outgoing edge to be added to the directed graph.
        /// </param>
        public void AddEdge(Node<TNodeValue, TEdgeLabel> from, Edge<TNodeValue, TEdgeLabel> edge)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (edge == null)
            {
                throw new ArgumentNullException(nameof(edge));
            }

            if (!nodes.Contains(from))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", nameof(from));
            }

            if (!nodes.Contains(edge.Target))
            {
                throw new ArgumentException("The target of the specified edge does not belong to the graph.", nameof(edge));
            }

            from.Successors.Add(edge);
        }

        /// <summary>
        /// Creates and inserts a labeled outgoing edge of the source node at the specified index.
        /// </summary>
        /// <param name="from">The node that is the source of the edge.</param>
        /// <param name="edgeIndex">
        /// The zero-based index at which the edge should be inserted on the successor list
        /// of the <paramref name="from"/> node.
        /// </param>
        /// <param name="to">The node that is the target of the edge.</param>
        /// <param name="label">The value of the edge label.</param>
        /// <returns>The created edge.</returns>
        public Edge<TNodeValue, TEdgeLabel> InsertEdge(Node<TNodeValue, TEdgeLabel> from, int edgeIndex, Node<TNodeValue, TEdgeLabel> to, TEdgeLabel label)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            if (!nodes.Contains(from))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", nameof(from));
            }

            if (!nodes.Contains(to))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", nameof(to));
            }

            if (edgeIndex < 0 || edgeIndex > from.Successors.Count)
            {
                throw new ArgumentOutOfRangeException("The specified edge index is out of range.", nameof(edgeIndex));
            }

            var edge = new Edge<TNodeValue, TEdgeLabel>(to, label);
            from.Successors.Insert(edgeIndex, edge);
            return edge;
        }

        /// <summary>
        /// Inserts a labeled outgoing edge of a source node at the specified index.
        /// </summary>
        /// <param name="from">The node that is the source of the edge.</param>
        /// <param name="edgeIndex">
        /// The zero-based index at which the edge should be inserted on the successor list
        /// of the <paramref name="from"/> node.
        /// </param>
        /// <param name="edge">
        /// The labeled outgoing edge that is to be inserted at the specified index on the successor
        /// list of the <paramref name="from"/> node.
        /// </param>
        public void InsertEdge(Node<TNodeValue, TEdgeLabel> from, int edgeIndex, Edge<TNodeValue, TEdgeLabel> edge)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (edge == null)
            {
                throw new ArgumentNullException(nameof(edge));
            }

            if (!nodes.Contains(from))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", nameof(from));
            }

            if (!nodes.Contains(edge.Target))
            {
                throw new ArgumentException("The target of the specified edge does not belong to the graph.", nameof(edge));
            }

            if (edgeIndex < 0 || edgeIndex > from.Successors.Count)
            {
                throw new ArgumentOutOfRangeException("The specified edge index is out of range.", nameof(edgeIndex));
            }

            from.Successors.Insert(edgeIndex, edge);
        }

        /// <summary>
        /// Creates and replaces a labeled outgoing edge of the source node at the specified index.
        /// </summary>
        /// <param name="from">The node that is the source of the edge.</param>
        /// <param name="edgeIndex">
        /// The zero-based index of the edge to replace on the successor list
        /// of the <paramref name="from"/> node.
        /// </param>
        /// <param name="to">The node that is the target of the edge.</param>
        /// <param name="label">The value of the edge label.</param>
        /// <returns>The created edge.</returns>
        public Edge<TNodeValue, TEdgeLabel> SetEdge(Node<TNodeValue, TEdgeLabel> from, int edgeIndex, Node<TNodeValue, TEdgeLabel> to, TEdgeLabel label)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(nameof(to));
            }

            if (!nodes.Contains(from))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", nameof(from));
            }

            if (!nodes.Contains(to))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", nameof(to));
            }

            if (edgeIndex < 0 || edgeIndex >= from.Successors.Count)
            {
                throw new ArgumentOutOfRangeException("The specified edge index is out of range.", nameof(edgeIndex));
            }

            var edge = new Edge<TNodeValue, TEdgeLabel>(to, label);
            from.Successors[edgeIndex] = edge;
            return edge;
        }

        /// <summary>
        /// Replaces a labeled outgoing edge of a source node at the specified index.
        /// </summary>
        /// <param name="from">The node that is the source of the edge.</param>
        /// <param name="edgeIndex">
        /// The zero-based index of the edge to replace on the successor list
        /// of the <paramref name="from"/> node.
        /// </param>
        /// <param name="edge">
        /// The labeled outgoing edge that is to be set at the specified index on the successor
        /// list of the <paramref name="from"/> node.
        /// </param>
        public void SetEdge(Node<TNodeValue, TEdgeLabel> from, int edgeIndex, Edge<TNodeValue, TEdgeLabel> edge)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (edge == null)
            {
                throw new ArgumentNullException(nameof(edge));
            }

            if (!nodes.Contains(from))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", nameof(from));
            }

            if (!nodes.Contains(edge.Target))
            {
                throw new ArgumentException("The target of the specified edge does not belong to the graph.", nameof(edge));
            }

            if (edgeIndex < 0 || edgeIndex >= from.Successors.Count)
            {
                throw new ArgumentOutOfRangeException("The specified edge index is out of range.", nameof(edgeIndex));
            }

            from.Successors[edgeIndex] = edge;
        }

        /// <summary>
        /// Determines whether a node is in the directed graph.
        /// </summary>
        /// <param name="node">The node to locate in the directed graph.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="node"/> is found in the
        /// directed graph; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(Node<TNodeValue, TEdgeLabel> node)
        {
            return nodes.Contains(node);
        }

        /// <summary>
        /// Removes the specified node from the directed graph.
        /// </summary>
        /// <param name="node">The node to remove from the directed graph.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="node"/> is successfully removed;
        /// otherwise, <see langword="false"/>. This method also returns <see langword="false"/>
        /// if <paramref name="node"/> was not found in the directed graph.
        /// </returns>
        public bool Remove(Node<TNodeValue, TEdgeLabel> node)
        {
            if (!nodes.Contains(node))
            {
                return false;
            }

            foreach (var n in nodes)
            {
                if (n == node) continue;
                for (int i = 0; i < n.Successors.Count; i++)
                {
                    if (n.Successors[i].Target == node)
                    {
                        n.Successors.RemoveAt(i);
                    }
                }
            }

            return nodes.Remove(node);
        }

        /// <summary>
        /// Removes the specified edge from the directed graph.
        /// </summary>
        /// <param name="from">The node that is the source of the edge.</param>
        /// <param name="edge">
        /// The outgoing edge to remove from the directed graph.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="edge"/> is successfully removed;
        /// otherwise, <see langword="false"/>. This method also returns <see langword="false"/>
        /// if <paramref name="edge"/> was not found in the directed graph.
        /// </returns>
        public bool RemoveEdge(Node<TNodeValue, TEdgeLabel> from, Edge<TNodeValue, TEdgeLabel> edge)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (edge == null)
            {
                throw new ArgumentNullException(nameof(edge));
            }

            if (!nodes.Contains(from))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", nameof(from));
            }

            if (!nodes.Contains(edge.Target))
            {
                throw new ArgumentException("The target of the specified edge does not belong to the graph.", nameof(edge));
            }

            return from.Successors.Remove(edge);
        }

        /// <summary>
        /// Removes all nodes and corresponding edges from the directed graph.
        /// </summary>
        public void Clear()
        {
            nodes.Clear();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the nodes in the
        /// directed graph.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the nodes in the
        /// directed graph.
        /// </returns>
        public IEnumerator<Node<TNodeValue, TEdgeLabel>> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }
    }
}
