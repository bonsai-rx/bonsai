using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Dag
{
    /// <summary>
    /// Provides a set of static methods for searching, sorting and manipulating directed graphs.
    /// </summary>
    public static class DirectedGraphExtensions
    {
        /// <summary>
        /// Returns the sequence of predecessors to the specified node.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">The source directed graph to search for predecessors.</param>
        /// <param name="node">The node for which to obtain the sequence of predecessors.</param>
        /// <returns>
        /// A sequence of <see cref="T:Bonsai.Dag.Node`2{T,U}"/> that contains all the predecessors
        /// to the specified node.
        /// </returns>
        public static IEnumerable<Node<TNodeValue, TEdgeLabel>> Predecessors<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source, Node<TNodeValue, TEdgeLabel> node)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (!source.Contains(node))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", "node");
            }

            foreach (var predecessor in source)
            {
                foreach (var successor in predecessor.Successors)
                {
                    if (successor.Target == node)
                    {
                        yield return predecessor;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the sequence of predecessor edges to the specified node.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">The source directed graph to search for predecessors.</param>
        /// <param name="node">The node for which to obtain the sequence of predecessors.</param>
        /// <returns>
        /// A sequence of triples containing the predecessor node, the edge linking the predecessor
        /// to the specified node and the edge index.
        /// </returns>
        public static IEnumerable<Tuple<Node<TNodeValue, TEdgeLabel>, Edge<TNodeValue, TEdgeLabel>, int>> PredecessorEdges<TNodeValue, TEdgeLabel>(
            this DirectedGraph<TNodeValue, TEdgeLabel> source,
            Node<TNodeValue, TEdgeLabel> node)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (!source.Contains(node))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", "node");
            }

            foreach (var predecessor in source)
            {
                int edgeIndex = 0;
                foreach (var successor in predecessor.Successors)
                {
                    if (successor.Target == node)
                    {
                        yield return Tuple.Create(predecessor, successor, edgeIndex);
                        break;
                    }

                    edgeIndex++;
                }
            }
        }

        /// <summary>
        /// Returns the sequence of successors to the specified node.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">The source directed graph to search for successors.</param>
        /// <param name="node">The node for which to obtain the sequence of successors.</param>
        /// <returns>
        /// A sequence of <see cref="T:Bonsai.Dag.Node`2{T,U}"/> that contains all the successors
        /// to the specified node.
        /// </returns>
        public static IEnumerable<Node<TNodeValue, TEdgeLabel>> Successors<TNodeValue, TEdgeLabel>(
            this DirectedGraph<TNodeValue, TEdgeLabel> source,
            Node<TNodeValue, TEdgeLabel> node)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (!source.Contains(node))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", "node");
            }

            foreach (var successor in node.Successors)
            {
                yield return successor.Target;
            }
        }

        /// <summary>
        /// Returns the sequence of all the nodes in the directed graph that have zero incoming edges.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">The directed graph to search for sources.</param>
        /// <returns>
        /// A sequence of <see cref="T:Bonsai.Dag.Node`2{T,U}"/> that contains all the nodes in the
        /// directed graph that have zero incoming edges.
        /// </returns>
        public static IEnumerable<Node<TNodeValue, TEdgeLabel>> Sources<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            foreach (var node in source)
            {
                if (!source.Predecessors(node).Any())
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// Returns the sequence of all the nodes in the directed graph that have zero outgoing edges.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">The directed graph to search for sinks.</param>
        /// <returns>
        /// A sequence of <see cref="T:Bonsai.Dag.Node`2{T,U}"/> that contains all the nodes in the
        /// directed graph that have zero outgoing edges.
        /// </returns>
        public static IEnumerable<Node<TNodeValue, TEdgeLabel>> Sinks<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            foreach (var node in source)
            {
                if (node.Successors.Count == 0)
                {
                    yield return node;
                }
            }
        }

        static IEnumerable<Node<TNodeValue, TEdgeLabel>> DepthFirstSearch<TNodeValue, TEdgeLabel>(Node<TNodeValue, TEdgeLabel> node, HashSet<Node<TNodeValue, TEdgeLabel>> visited, Stack<Node<TNodeValue, TEdgeLabel>> stack)
        {
            if (visited.Contains(node)) yield break;
            stack.Push(node);

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                if (!visited.Contains(current))
                {
                    visited.Add(current);
                    for (int i = current.Successors.Count - 1; i >= 0; i--)
                    {
                        if (visited.Contains(current.Successors[i].Target)) continue;
                        stack.Push(current.Successors[i].Target);
                    }
                }
                else yield return stack.Pop();
            }
        }

        /// <summary>
        /// Traverses through all the directed graph nodes in depth-first order, starting from
        /// the specified node.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="node">The node from which to start the search.</param>
        /// <returns>
        /// A sequence of <see cref="T:Bonsai.Dag.Node`2{T,U}"/> that contains the set of all nodes
        /// reachable from <paramref name="node"/> in depth-first order.
        /// </returns>
        public static IEnumerable<Node<TNodeValue, TEdgeLabel>> DepthFirstSearch<TNodeValue, TEdgeLabel>(this Node<TNodeValue, TEdgeLabel> node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            var visited = new HashSet<Node<TNodeValue, TEdgeLabel>>();
            var stack = new Stack<Node<TNodeValue, TEdgeLabel>>();
            return DepthFirstSearch(node, visited, stack);
        }

        /// <summary>
        /// Traverses through all the directed graph nodes in depth-first order.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">The source directed graph that will be traversed.</param>
        /// <returns>
        /// A sequence of <see cref="T:Bonsai.Dag.Node`2{T,U}"/> that contains the set of all
        /// graph nodes in depth-first order.
        /// </returns>
        public static IEnumerable<Node<TNodeValue, TEdgeLabel>> DepthFirstSearch<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var visited = new HashSet<Node<TNodeValue, TEdgeLabel>>();
            var stack = new Stack<Node<TNodeValue, TEdgeLabel>>();
            return from root in source
                   from node in DepthFirstSearch(root, visited, stack)
                   select node;
        }

        /// <summary>
        /// Traverses through all the directed graph nodes in such a way as to guarantee that for
        /// every node in the sequence, all its predecessors have been visited first.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">The source directed graph that will be traversed.</param>
        /// <returns>
        /// A sequence of <see cref="T:Bonsai.Dag.Node`2{T,U}"/> that contains the set of all
        /// graph nodes in topological sort order.
        /// </returns>
        public static IEnumerable<Node<TNodeValue, TEdgeLabel>> TopologicalSort<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            IEnumerable<Node<TNodeValue, TEdgeLabel>> topologicalOrder;
            if (!Bonsai.Dag.TopologicalSort.TrySort(source, out topologicalOrder))
            {
                return Enumerable.Empty<Node<TNodeValue, TEdgeLabel>>();
            }
            return topologicalOrder;
        }

        /// <summary>
        /// Determines whether a directed graph is acyclic.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">The source directed graph to test.</param>
        /// <returns>
        /// <b>true</b> if <paramref name="source"/> has no cycles; otherwise, <b>false</b>.
        /// </returns>
        public static bool Acyclic<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            IEnumerable<Node<TNodeValue, TEdgeLabel>> topologicalOrder;
            return source.Count == 0 || Bonsai.Dag.TopologicalSort.TrySort(source, out topologicalOrder);
        }

        /// <summary>
        /// Creates a serializable descriptor from a <see cref="T:Bonsai.Dag.DirectedGraph`2{T,U}"/>.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">A <see cref="T:Bonsai.Dag.DirectedGraph`2{T,U}"/> to create a descriptor from.</param>
        /// <returns>A serializable descriptor that contains all the node and edge label values.</returns>
        [Obsolete]
        public static DirectedGraphDescriptor<TNodeValue, TEdgeLabel> ToDescriptor<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var descriptor = new DirectedGraphDescriptor<TNodeValue, TEdgeLabel>();
            ToDescriptor(source, descriptor);
            return descriptor;
        }

        /// <summary>
        /// Adds nodes to a serializable descriptor from a <see cref="T:Bonsai.Dag.DirectedGraph`2{T,U}"/>.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">A <see cref="T:Bonsai.Dag.DirectedGraph`2{T,U}"/> to create descriptors from.</param>
        /// <param name="descriptor">The serializable descriptor to add node descriptors to.</param>
        [Obsolete]
        public static void ToDescriptor<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source, DirectedGraphDescriptor<TNodeValue, TEdgeLabel> descriptor)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            var nodes = source.ToArray();

            foreach (var node in nodes)
            {
                descriptor.Nodes.Add(node.Value);
            }

            var from = 0;
            foreach (var node in nodes)
            {
                foreach (var successor in node.Successors)
                {
                    var to = Array.IndexOf(nodes, successor.Target);
                    descriptor.Edges.Add(new EdgeDescriptor<TEdgeLabel>(from, to, successor.Label));
                }

                from++;
            }
        }

        /// <summary>
        /// Adds the contents of the specified graph descriptor to the <see cref="T:Bonsai.Dag.DirectedGraph`2{T,U}"/>.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">
        /// The directed graph on which to add the contents of <paramref name="descriptor"/>.
        /// </param>
        /// <param name="descriptor">
        /// The serializable descriptor whose contents should be added to the
        /// <see cref="T:Bonsai.Dag.DirectedGraph`2{T,U}"/>.
        /// </param>
        [Obsolete]
        public static void AddDescriptor<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source, DirectedGraphDescriptor<TNodeValue, TEdgeLabel> descriptor)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException("descriptor");
            }

            var nodes = descriptor.Nodes.Select(value => source.Add(value)).ToArray();
            foreach (var edge in descriptor.Edges)
            {
                source.AddEdge(nodes[edge.From], nodes[edge.To], edge.Label);
            }
        }

        /// <summary>
        /// Creates a directed graph from a <see cref="T:Bonsai.Dag.DirectedGraphDescriptor`2{T,U}"/>.
        /// </summary>
        /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
        /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
        /// <param name="source">
        /// A <see cref="T:Bonsai.Dag.DirectedGraphDescriptor`2{T,U}"/> to create a directed graph from.
        /// </param>
        /// <returns>
        /// A <see cref="T:Bonsai.Dag.DirectedGraph`2{T,U}"/> that contains all the node
        /// and edge label values specified in the descriptor.
        /// </returns>
        [Obsolete]
        public static DirectedGraph<TNodeValue, TEdgeLabel> ToDirectedGraph<TNodeValue, TEdgeLabel>(this DirectedGraphDescriptor<TNodeValue, TEdgeLabel> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var graph = new DirectedGraph<TNodeValue, TEdgeLabel>();
            graph.AddDescriptor(source);
            return graph;
        }
    }
}
