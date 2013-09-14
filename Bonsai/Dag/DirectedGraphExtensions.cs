using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Dag
{
    public static class DirectedGraphExtensions
    {
        public static IEnumerable<Node<TValue, TLabel>> Predecessors<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source, Node<TValue, TLabel> node)
        {
            if (!source.Contains(node))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", "node");
            }

            foreach (var predecessor in source)
            {
                foreach (var successor in predecessor.Successors)
                {
                    if (successor.Node == node)
                    {
                        yield return predecessor;
                        break;
                    }
                }
            }
        }

        public static IEnumerable<Tuple<Node<TValue, TLabel>, Edge<TValue, TLabel>, int>> PredecessorEdges<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source, Node<TValue, TLabel> node)
        {
            if (!source.Contains(node))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", "node");
            }

            foreach (var predecessor in source)
            {
                int edgeIndex = 0;
                foreach (var successor in predecessor.Successors)
                {
                    if (successor.Node == node)
                    {
                        yield return Tuple.Create(predecessor, successor, edgeIndex);
                        break;
                    }

                    edgeIndex++;
                }
            }
        }

        public static IEnumerable<Node<TValue, TLabel>> Successors<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source, Node<TValue, TLabel> node)
        {
            if (!source.Contains(node))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", "node");
            }

            foreach (var successor in node.Successors)
            {
                yield return successor.Node;
            }
        }

        public static IEnumerable<Node<TValue, TLabel>> Sources<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source)
        {
            foreach (var node in source)
            {
                if (!source.Predecessors(node).Any())
                {
                    yield return node;
                }
            }
        }

        public static IEnumerable<Node<TValue, TLabel>> Sinks<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source)
        {
            foreach (var node in source)
            {
                if (node.Successors.Count == 0)
                {
                    yield return node;
                }
            }
        }

        public static IEnumerable<Node<TValue, TLabel>> DepthFirstSearch<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source)
        {
            var visited = new HashSet<Node<TValue, TLabel>>();
            var stack = new Stack<Node<TValue, TLabel>>();
            foreach (var node in source)
            {
                if (visited.Contains(node)) continue;
                stack.Push(node);

                while (stack.Count > 0)
                {
                    var current = stack.Peek();
                    if (!visited.Contains(current))
                    {
                        visited.Add(current);
                        foreach (var successor in current.Successors)
                        {
                            if (visited.Contains(successor.Node)) continue;
                            stack.Push(successor.Node);
                        }
                    }
                    else yield return stack.Pop();
                }
            }
        }

        public static IEnumerable<Node<TValue, TLabel>> TopologicalSort<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source)
        {
            var predecessorCount = new Dictionary<Node<TValue, TLabel>, int>();
            var ordering = new List<Node<TValue, TLabel>>(source.Count);

            foreach (var node in source)
            {
                foreach (var successor in node.Successors)
                {
                    int count;
                    predecessorCount.TryGetValue(successor.Node, out count);
                    predecessorCount[successor.Node] = count + 1;
                    ordering.Remove(successor.Node);
                }

                if (!predecessorCount.ContainsKey(node))
                {
                    predecessorCount[node] = 0;
                    ordering.Add(node);
                }
            }

            for (int i = 0; i < ordering.Count; i++)
            {
                var node = ordering[i];

                foreach (var successor in node.Successors)
                {
                    if (--predecessorCount[successor.Node] == 0)
                    {
                        ordering.Add(successor.Node);
                    }
                }
            }

            if (ordering.Count < ordering.Capacity) return Enumerable.Empty<Node<TValue, TLabel>>();
            return ordering;
        }

        public static bool Acyclic<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source)
        {
            return source.Count == 0 || source.TopologicalSort().Any();
        }

        public static DirectedGraphDescriptor<TValue, TLabel> ToDescriptor<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source)
        {
            var descriptor = new DirectedGraphDescriptor<TValue, TLabel>();
            ToDescriptor(source, descriptor);
            return descriptor;
        }

        public static void ToDescriptor<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source, DirectedGraphDescriptor<TValue, TLabel> descriptor)
        {
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
                    var to = Array.IndexOf(nodes, successor.Node);
                    descriptor.Edges.Add(new EdgeDescriptor<TLabel>(from, to, successor.Label));
                }

                from++;
            }
        }

        public static void AddDescriptor<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source, DirectedGraphDescriptor<TValue, TLabel> descriptor)
        {
            var nodes = descriptor.Nodes.Select(value => new Node<TValue, TLabel>(value)).ToArray();

            foreach (var node in nodes)
            {
                source.Add(node);
            }

            foreach (var edge in descriptor.Edges)
            {
                source.AddEdge(nodes[edge.From], nodes[edge.To], edge.Label);
            }
        }

        public static DirectedGraph<TValue, TLabel> ToDirectedGraph<TValue, TLabel>(this DirectedGraphDescriptor<TValue, TLabel> source)
        {
            var graph = new DirectedGraph<TValue, TLabel>();
            graph.AddDescriptor(source);
            return graph;
        }
    }
}
