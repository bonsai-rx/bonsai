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
            var orderedRoots = new SortedList<int, SortedNode>();
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
                                orderedRoots.Add(source.Count - rootIndices[current.Node], nodeMark);
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

            topologicalOrder = ResultIterator(orderedRoots.Values);
            return true;
        }

        static IEnumerable<Node<TNodeValue, TEdgeLabel>> ResultIterator(IList<SortedNode> source)
        {
            var stack = new Stack<SortedNode>();
            var result = new List<Node<TNodeValue, TEdgeLabel>>(source.Count);
            foreach (var root in source)
            {
                stack.Push(root);
                while (stack.Count > 0)
                {
                    var current = stack.Pop();
                    if (--current.Flag <= 0)
                    {
                        result.Add(current.Node);
                        foreach (var dependency in current.GetDependencies())
                        {
                            stack.Push(dependency);
                        }
                    }
                }
            }

            return ReverseIterator(result);
        }

        static IEnumerable<TSource> ReverseIterator<TSource>(List<TSource> source)
        {
            for (int i = source.Count - 1; i >= 0; i--)
            {
                yield return source[i];
            }
        }

        [DebuggerDisplay("({Node}, Flag:{Flag})")]
        class SortedNode
        {
            List<NodeDependency> dependencies;
            public Node<TNodeValue, TEdgeLabel> Node;
            public int Flag;

            public void AddDependency(TEdgeLabel key, SortedNode dependency)
            {
                NodeDependency nodeDependency;
                nodeDependency.Key = key;
                nodeDependency.Dependency = dependency;
                dependencies ??= new List<NodeDependency>();
                dependencies.Add(nodeDependency);
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
    }
}
