using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var stack = new CallStack(source.Count);
            var marks = new MarkDictionary(source.Count);
            var orderingStack = new Stack<ResultOrdering>();

            foreach (var node in source)
            {
                if (!marks.ContainsKey(node))
                {
                    var ordering = new ResultOrdering();
                    orderingStack.Push(ordering);
                    stack.Push(node, 0);
                    while (stack.Count > 0)
                    {
                        var current = stack.Pop();
                        if (current.Index == 0)
                        {
                            marks.Add(current.Node, ordering, TemporaryMark);
                        }

                        var successors = current.Node.Successors;
                        if (current.Index >= successors.Count)
                        {
                            ordering.Add(current.Node);
                            marks.Add(current.Node, ordering, ordering.Count);
                        }
                        else
                        {
                            NodeMark successorMark;
                            stack.Push(current.Node, current.Index + 1);
                            var successor = successors[successors.Count - current.Index - 1].Target;
                            if (marks.TryGetValue(successor, out successorMark))
                            {
                                if (successorMark.Flag == TemporaryMark)
                                {
                                    topologicalOrder = null;
                                    return false;
                                }

                                if (successorMark.Ordering != ordering)
                                {
                                    ordering.Add(successorMark.Ordering, successorMark.Flag);
                                }
                            }
                            else stack.Push(successor, 0);
                        }
                    }
                }
            }

            var result = new List<Node<TNodeValue, TEdgeLabel>>(source.Count);
            while (orderingStack.Count > 0)
            {
                var ordering = orderingStack.Pop();
                ordering.Evaluate(result);
            }

            topologicalOrder = ReverseIterator(result);
            return true;
        }

        static IEnumerable<TSource> ReverseIterator<TSource>(List<TSource> source)
        {
            for (int i = source.Count - 1; i >= 0; i--)
            {
                yield return source[i];
            }
        }

        class CallStack : Stack<NodeCall>
        {
            public CallStack(int capacity)
                : base(capacity)
            {
            }

            public void Push(Node<TNodeValue, TEdgeLabel> node, int index)
            {
                NodeCall item;
                item.Node = node;
                item.Index = index;
                Push(item);
            }
        }

        class MarkDictionary : Dictionary<Node<TNodeValue, TEdgeLabel>, NodeMark>
        {
            public MarkDictionary(int capacity)
                : base(capacity)
            {
            }

            public void Add(Node<TNodeValue, TEdgeLabel> node, ResultOrdering ordering, int flag)
            {
                NodeMark mark;
                mark.Ordering = ordering;
                mark.Flag = flag;
                this[node] = mark;
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        class ResultOrdering
        {
            int frontIndex;
            readonly List<Action<List<Node<TNodeValue, TEdgeLabel>>>> ordering;

            public ResultOrdering()
            {
                ordering = new List<Action<List<Node<TNodeValue, TEdgeLabel>>>>();
            }

            public int Count
            {
                get { return ordering.Count; }
            }

            public void Add(Node<TNodeValue, TEdgeLabel> node)
            {
                ordering.Add(result => result.Add(node));
            }

            public void Add(ResultOrdering ordering, int index)
            {
                this.ordering.Add(result => ordering.Evaluate(result, index));
            }

            public void Evaluate(List<Node<TNodeValue, TEdgeLabel>> result)
            {
                Evaluate(result, ordering.Count);
            }

            public void Evaluate(List<Node<TNodeValue, TEdgeLabel>> result, int index)
            {
                while (frontIndex < index)
                {
                    ordering[frontIndex](result);
                    frontIndex++;
                }
            }
        }

        [DebuggerDisplay("({Node}, Index:{Index})")]
        struct NodeCall
        {
            public Node<TNodeValue, TEdgeLabel> Node;
            public int Index;
        }

        struct NodeMark
        {
            public ResultOrdering Ordering;
            public int Flag;
        }
    }
}
