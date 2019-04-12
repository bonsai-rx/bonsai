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
            var orderedComponents = new List<ComponentOrdering>();

            foreach (var node in source)
            {
                if (!marks.ContainsKey(node))
                {
                    var ordering = new NodeOrdering();
                    var component = default(ComponentOrdering);
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
                            var successorIndex = successors.Count - current.Index - 1;
                            var successor = successors[successorIndex].Target;
                            if (marks.TryGetValue(successor, out successorMark))
                            {
                                if (successorMark.Flag == TemporaryMark)
                                {
                                    topologicalOrder = null;
                                    return false;
                                }

                                if (successorMark.Ordering != ordering)
                                {
                                    foreach (var dependency in successorMark.Dependencies)
                                    {
                                        if (dependency.Ordering == ordering) continue;
                                        ordering.Add(dependency.Ordering, dependency.Flag);
                                    }

                                    ordering.Add(successorMark.Ordering, successorMark.Flag);
                                    if (component == null) component = successorMark.Ordering.Component;
                                    else if (component != successorMark.Ordering.Component)
                                    {
                                        successorMark.Ordering.Component.Add(component);
                                        orderedComponents[component.Index] = null;
                                        component = successorMark.Ordering.Component;
                                    }

                                    for (int i = successors.Count - 1; i > successorIndex; i--)
                                    {
                                        var sibling = successors[i].Target;
                                        var siblingMark = marks[sibling];
                                        successorMark.AddDependency(siblingMark);
                                    }
                                }
                            }
                            else stack.Push(successor, 0);
                        }
                    }

                    if (component == null)
                    {
                        component = new ComponentOrdering(orderedComponents.Count);
                        orderedComponents.Add(component);
                    }
                    component.Add(ordering);
                }
            }

            var result = new List<Node<TNodeValue, TEdgeLabel>>(source.Count);
            for (int i = orderedComponents.Count - 1; i >= 0; i--)
            {
                if (orderedComponents[i] == null) continue;
                orderedComponents[i].Evaluate(result);
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

        [DebuggerDisplay("Index = {Index}")]
        class ComponentOrdering
        {
            readonly List<NodeOrdering> orderingStack;
            readonly int index;

            public ComponentOrdering(int componentIndex)
            {
                orderingStack = new List<NodeOrdering>();
                index = componentIndex;
            }

            public int Index
            {
                get { return index; }
            }

            public void Add(NodeOrdering ordering)
            {
                orderingStack.Add(ordering);
                ordering.Component = this;
            }

            public void Add(ComponentOrdering component)
            {
                for (int i = 0; i < component.orderingStack.Count; i++)
                {
                    Add(component.orderingStack[i]);
                }
            }

            public void Evaluate(List<Node<TNodeValue, TEdgeLabel>> result)
            {
                for (int i = orderingStack.Count - 1; i >= 0; i--)
                {
                    orderingStack[i].Evaluate(result);
                }
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

            public void Add(Node<TNodeValue, TEdgeLabel> node, NodeOrdering ordering, int flag)
            {
                var mark = new NodeMark();
                mark.Ordering = ordering;
                mark.Flag = flag;
                this[node] = mark;
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        class NodeOrdering
        {
            int frontIndex;
            readonly List<Action<List<Node<TNodeValue, TEdgeLabel>>>> ordering;

            public NodeOrdering()
            {
                ordering = new List<Action<List<Node<TNodeValue, TEdgeLabel>>>>();
            }

            public int Count
            {
                get { return ordering.Count; }
            }

            public ComponentOrdering Component { get; set; }

            public void Add(Node<TNodeValue, TEdgeLabel> node)
            {
                ordering.Add(result => result.Add(node));
            }

            public void Add(NodeOrdering ordering, int index)
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

        class NodeMark
        {
            List<NodeMark> dependencies;
            public NodeOrdering Ordering;
            public int Flag;

            public void AddDependency(NodeMark dependency)
            {
                if (dependencies == null) dependencies = new List<NodeMark>();
                dependencies.Add(dependency);
            }

            public IEnumerable<NodeMark> Dependencies
            {
                get { return dependencies ?? Enumerable.Empty<NodeMark>(); }
            }
        }
    }
}
