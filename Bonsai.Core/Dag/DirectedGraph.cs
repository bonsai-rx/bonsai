using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Dag
{
    /// <summary>
    /// Represents a directed graph that consists of labeled nodes and edges.
    /// </summary>
    /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
    /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
    public class DirectedGraph<TNodeValue, TEdgeLabel>
        : ICollection<Node<TNodeValue, TEdgeLabel>>
        , IReadOnlyList<Node<TNodeValue, TEdgeLabel>>
    {
        readonly List<Node<TNodeValue, TEdgeLabel>> nodes;
        readonly HashSet<Node<TNodeValue, TEdgeLabel>> nodeLookup;
        readonly IComparer<TNodeValue> valueComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{T, U}"/> class.
        /// </summary>
        public DirectedGraph()
        {
            nodes = new List<Node<TNodeValue, TEdgeLabel>>();
            nodeLookup = new HashSet<Node<TNodeValue, TEdgeLabel>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectedGraph{T, U}"/> class
        /// that uses a specified comparer.
        /// </summary>
        /// <param name="comparer">The optional comparer to use for ordering node values.</param>
        [Obsolete]
        public DirectedGraph(IComparer<TNodeValue> comparer)
            : this()
        {
            valueComparer = comparer;
        }

        /// <summary>
        /// Gets the optional <see cref="IComparer{TNodeValue}"/> object used to determine
        /// the order of the values in the directed graph.
        /// </summary>
        [Obsolete]
        public virtual IComparer<TNodeValue> Comparer
        {
            get { return valueComparer; }
        }

        /// <summary>
        /// Gets the number of nodes in the directed graph.
        /// </summary>
        public int Count
        {
            get { return nodes.Count; }
        }

        /// <summary>
        /// Gets the node at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the node to get.</param>
        /// <returns>The node at the specified index.</returns>
        public Node<TNodeValue, TEdgeLabel> this[int index]
        {
            get { return nodes[index]; }
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
        /// Adds a node and all its successors to the directed graph.
        /// </summary>
        /// <param name="node">The node to be added to the directed graph.</param>
        public void Add(Node<TNodeValue, TEdgeLabel> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (nodeLookup.Add(node))
            {
                nodes.Add(node);
                foreach (var successor in node.Successors)
                {
                    Add(successor.Target);
                }
            }
        }

        void ThrowIfEdgeVerticesNullOrNotInGraph(Node<TNodeValue, TEdgeLabel> from, Node<TNodeValue, TEdgeLabel> to, string targetParamName)
        {
            if (from == null)
            {
                throw new ArgumentNullException(nameof(from));
            }

            if (to == null)
            {
                throw new ArgumentNullException(targetParamName);
            }

            if (!nodeLookup.Contains(from))
            {
                throw new ArgumentException("The specified edge source does not belong to the graph.", nameof(from));
            }

            if (!nodeLookup.Contains(to))
            {
                throw new ArgumentException("The specified edge target does not belong to the graph.", targetParamName);
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
            ThrowIfEdgeVerticesNullOrNotInGraph(from, to, nameof(to));
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
            ThrowIfEdgeVerticesNullOrNotInGraph(from, edge?.Target, nameof(edge));
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
            ThrowIfEdgeVerticesNullOrNotInGraph(from, to, nameof(to));
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
            ThrowIfEdgeVerticesNullOrNotInGraph(from, edge?.Target, nameof(edge));
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
            ThrowIfEdgeVerticesNullOrNotInGraph(from, to, nameof(to));
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
            ThrowIfEdgeVerticesNullOrNotInGraph(from, edge?.Target, nameof(edge));
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
            return nodeLookup.Contains(node);
        }

        /// <summary>
        /// Searches for the specified node and returns its zero-based
        /// index within the collection.
        /// </summary>
        /// <param name="node">The node to locate in the collection.</param>
        /// <returns>
        /// The zero-based index of <paramref name="node"/> in the collection,
        /// if found; otherwise, -1.
        /// </returns>
        public int IndexOf(Node<TNodeValue, TEdgeLabel> node)
        {
            if (nodeLookup.Contains(node))
            {
                return nodes.IndexOf(node);
            }

            return -1;
        }

        /// <summary>
        /// Creates and inserts a new node with the specified value into the
        /// directed graph at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the node should be inserted.</param>
        /// <param name="value">The value of the node label.</param>
        /// <returns>The created node.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public Node<TNodeValue, TEdgeLabel> Insert(int index, TNodeValue value)
        {
            if (index < 0 || index > nodes.Count)
            {
                throw new ArgumentOutOfRangeException("The specified index is out of range.", nameof(index));
            }

            var node = new Node<TNodeValue, TEdgeLabel>(value);
            nodes.Insert(index, node);
            nodeLookup.Add(node);
            return node;
        }

        /// <summary>
        /// Inserts a node and all its successors into the directed graph at the
        /// specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the node should be inserted.</param>
        /// <param name="node">The node to insert into the directed graph.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <remarks>
        /// If the node or any of its successors are already in the directed graph,
        /// they will be moved into the new position, in depth-first order.
        /// </remarks>
        public void Insert(int index, Node<TNodeValue, TEdgeLabel> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (index < 0 || index > nodes.Count)
            {
                throw new ArgumentOutOfRangeException("The specified index is out of range.", nameof(index));
            }

            InsertInternal(index, new[] { node });
        }

        /// <summary>
        /// Inserts all the nodes in a collection and their successors into the
        /// directed graph at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which the node collection should be inserted.
        /// </param>
        /// <param name="collection">
        /// The collection of nodes to insert into the directed graph.
        /// </param>
        /// <remarks>
        /// If any of the nodes in the collection, or their successors, are already
        /// in the directed graph, they will be moved into the new index position,
        /// in depth-first order.
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void InsertRange(int index, IEnumerable<Node<TNodeValue, TEdgeLabel>> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (index < 0 || index > nodes.Count)
            {
                throw new ArgumentOutOfRangeException("The specified index is out of range.", nameof(index));
            }

            InsertInternal(index, collection);
        }

        void InsertInternal(int index, IEnumerable<Node<TNodeValue, TEdgeLabel>> collection)
        {
            var nodeIndex = 0;
            var insertionIndex = index;
            var inserted = new HashSet<Node<TNodeValue, TEdgeLabel>>(collection);
            var additionalNodes = default(List<Node<TNodeValue, TEdgeLabel>>);
            var visited = new HashSet<Node<TNodeValue, TEdgeLabel>>();
            var stack = new Stack<Node<TNodeValue, TEdgeLabel>>();
            foreach (var node in inserted)
            {
                foreach (var successor in node.DepthFirstSearch(visited, stack))
                {
                    if (!inserted.Contains(successor) && !Contains(successor))
                    {
                        additionalNodes ??= new();
                        additionalNodes.Add(successor);
                    }
                }
            }

            if (additionalNodes != null)
            {
                inserted.UnionWith(additionalNodes);
            }

            nodes.RemoveAll(node =>
            {
                var remove = inserted.Contains(node);
                if (remove && nodeIndex < index)
                {
                    insertionIndex--;
                }

                nodeIndex++;
                return remove;
            });
            nodes.InsertRange(insertionIndex, inserted);
            nodeLookup.UnionWith(inserted);
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
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (nodeLookup.Remove(node))
            {
                nodes.RemoveAll(n =>
                {
                    if (n == node)
                    {
                        return true;
                    }

                    for (int i = 0; i < n.Successors.Count; i++)
                    {
                        if (n.Successors[i].Target == node)
                        {
                            n.Successors.RemoveAt(i);
                        }
                    }
                    return false;
                });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the node at the specified index of the directed graph.
        /// </summary>
        /// <param name="index">The zero-based index of the node to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= nodes.Count)
            {
                throw new ArgumentOutOfRangeException("The specified index is out of range.", nameof(index));
            }

            var node = nodes[index];
            Remove(node);
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
            ThrowIfEdgeVerticesNullOrNotInGraph(from, edge?.Target, nameof(edge));
            return from.Successors.Remove(edge);
        }

        /// <summary>
        /// Removes all nodes and corresponding edges from the directed graph.
        /// </summary>
        public void Clear()
        {
            nodes.Clear();
            nodeLookup.Clear();
        }

        /// <summary>
        /// Copies all the nodes in the directed graph to a compatible
        /// one-dimensional array, starting at the beginning of the target array.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional array that is the destination of the nodes
        /// copied from the directed graph.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of nodes in the directed graph is greater than the number
        /// of elements that the destination array can contain.
        /// </exception>
        public void CopyTo(Node<TNodeValue, TEdgeLabel>[] array)
        {
            nodes.CopyTo(array);
        }

        /// <summary>
        /// Copies all the nodes in the directed graph to a compatible
        /// one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional array that is the destination of the nodes
        /// copied from the directed graph.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in the array at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of nodes in the directed graph is greater than the available
        /// space from <paramref name="arrayIndex"/> to the end of the destination array.
        /// </exception>
        public void CopyTo(Node<TNodeValue, TEdgeLabel>[] array, int arrayIndex)
        {
            nodes.CopyTo(array, arrayIndex);
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
            return valueComparer != null
                ? nodes.OrderBy(node => node.Value, valueComparer).GetEnumerator()
                : nodes.GetEnumerator();
        }

        bool ICollection<Node<TNodeValue, TEdgeLabel>>.IsReadOnly
        {
            get { return false; }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }
    }
}
