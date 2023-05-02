using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Bonsai.Dag
{
    static class TopologicalSort
    {
        public static bool TrySort<TNodeValue, TEdgeLabel>(
            DirectedGraph<TNodeValue, TEdgeLabel> source,
            out IEnumerable<Node<TNodeValue, TEdgeLabel>> topologicalOrder)
        {
            return TopologicalSort<TNodeValue, TEdgeLabel>.TrySort(source, out topologicalOrder);
        }
    }

    static class TopologicalSort<TNodeValue, TEdgeLabel>
    {
        const int TemporaryMark = -1;

        public static bool TrySort(
            DirectedGraph<TNodeValue, TEdgeLabel> source,
            out IEnumerable<Node<TNodeValue, TEdgeLabel>> topologicalOrder)
        {
            var stack = new CallStack();
            var marks = new MarkDictionary(source.Count);
            var orderedRoots = new List<SortedNode>();
            var rootIndices = new Dictionary<Node<TNodeValue, TEdgeLabel>, int>();
            for (int i = 0; i < source.Count; i++)
            {
                var node = source[i];
                if (node.Successors.Count == 0)
                {
                    rootIndices[node] = i;
                }
            }

            foreach (var node in source)
            {
                if (!marks.ContainsKey(node))
                {
                    stack.Push(node, 0);
                    while (stack.Count > 0)
                    {
                        var current = stack.Pop();
                        if (current.Index == 0)
                        {
                            marks.Add(current.Node, TemporaryMark);
                        }

                        var successors = current.Node.Successors;
                        if (current.Index >= successors.Count)
                        {
                            var nodeMark = marks.Add(current.Node, successors.Count);
                            if (nodeMark.Flag == 0)
                            {
                                var rank = source.Count - rootIndices[current.Node];
                                nodeMark.Rank = rank;
                                nodeMark.Component = new ConnectedComponent(rank) { nodeMark };
                                orderedRoots.Add(nodeMark);
                            }
                            else
                            {
                                foreach (var successor in successors)
                                {
                                    marks[successor.Target].AddDependency(successor.Label, nodeMark);
                                }
                            }
                        }
                        else
                        {
                            stack.Push(current.Node, current.Index + 1);
                            var successor = successors[current.Index].Target;
                            if (marks.TryGetValue(successor, out SortedNode successorMark))
                            {
                                if (successorMark.Flag == TemporaryMark)
                                {
                                    topologicalOrder = null;
                                    return false;
                                }
                            }
                            else stack.Push(successor, 0);
                        }
                    }
                }
            }

            orderedRoots.Sort((x, y) =>
            {
                var comparison = Comparer<int>.Default.Compare(x.Component.Rank, y.Component.Rank);
                if (comparison == 0) comparison = Comparer<int>.Default.Compare(x.Rank, y.Rank);
                return comparison;
            });
            topologicalOrder = ResultIterator(orderedRoots);
            return true;
        }

        static IEnumerable<Node<TNodeValue, TEdgeLabel>> ResultIterator(IReadOnlyList<SortedNode> source)
        {
            var stack = new Stack<SortedNode>();
            var result = new Stack<Node<TNodeValue, TEdgeLabel>>(source.Count);
            foreach (var root in source)
            {
                stack.Push(root);
                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    if (--current.Flag <= 0)
                    {
                        result.Push(current.Node);
                        foreach (var dependency in current.GetDependencies())
                        {
                            stack.Push(dependency);
                        }
                    }
                }
            }

            return result;
        }

        [DebuggerDisplay("({Node}, Flag:{Flag})")]
        class SortedNode
        {
            List<NodeDependency> dependencies;
            public ConnectedComponent Component;
            public Node<TNodeValue, TEdgeLabel> Node;
            public int Rank = -1;
            public int Flag;

            public void AddDependency(TEdgeLabel key, SortedNode dependency)
            {
                NodeDependency nodeDependency;
                nodeDependency.Key = key;
                nodeDependency.Dependency = dependency;
                dependencies ??= new List<NodeDependency>();
                dependencies.Add(nodeDependency);
                if (dependency.Component == null)
                {
                    dependency.Component = Component;
                }
                else if (Component != dependency.Component)
                {
                    ConnectedComponent high, low;
                    if (Component.Rank > dependency.Component.Rank)
                    {
                        high = Component;
                        low = dependency.Component;
                    }
                    else
                    {
                        high = dependency.Component;
                        low = Component;
                    }

                    foreach (var root in low)
                    {
                        high.Add(root);
                        root.Component = high;
                    }

                    Component = dependency.Component = high;
                }
            }

            public IEnumerable<SortedNode> GetDependencies()
            {
                if (dependencies == null)
                {
                    return Enumerable.Empty<SortedNode>();
                }

                dependencies.Sort((x, y) => Comparer<TEdgeLabel>.Default.Compare(x.Key, y.Key));
                return dependencies.Select(x => x.Dependency);
            }

            struct NodeDependency
            {
                public SortedNode Dependency;
                public TEdgeLabel Key;
            }
        }

        class MarkDictionary : Dictionary<Node<TNodeValue, TEdgeLabel>, SortedNode>
        {
            public MarkDictionary(int capacity)
                : base(capacity)
            {
            }

            public SortedNode Add(Node<TNodeValue, TEdgeLabel> node, int flag)
            {
                if (!TryGetValue(node, out SortedNode mark))
                {
                    mark = new SortedNode();
                    mark.Node = node;
                    Add(node, mark);
                }

                mark.Flag = flag;
                return mark;
            }
        }

        class CallStack : Stack<NodeCall>
        {
            public void Push(Node<TNodeValue, TEdgeLabel> node, int index)
            {
                NodeCall item;
                item.Node = node;
                item.Index = index;
                Push(item);
            }
        }

        [DebuggerDisplay("({Node}, Index:{Index})")]
        struct NodeCall
        {
            public Node<TNodeValue, TEdgeLabel> Node;
            public int Index;
        }

        class ConnectedComponent : HashSet<SortedNode>
        {
            internal ConnectedComponent(int rank)
            {
                Rank = rank;
            }

            internal int Rank { get; }
        }
    }
}
