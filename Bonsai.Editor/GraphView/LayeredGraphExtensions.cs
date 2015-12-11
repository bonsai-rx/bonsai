using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;
using System.Globalization;
using Bonsai.Expressions;
using System.ComponentModel;

namespace Bonsai.Design
{
    static class LayeredGraphExtensions
    {
        public static WorkflowBuilder ToWorkflowBuilder(this IEnumerable<GraphNode> source)
        {
            return ToWorkflowBuilder(source, true);
        }

        public static WorkflowBuilder ToWorkflowBuilder(this IEnumerable<GraphNode> source, bool recurse)
        {
            var workflow = source.Select(node => (Node<ExpressionBuilder, ExpressionBuilderArgument>)node.Tag)
                                 .FromInspectableGraph(recurse);
            return new WorkflowBuilder(workflow);
        }

        public static IEnumerable<GraphNode> LayeredNodes(this IEnumerable<GraphNodeGrouping> source)
        {
            return source.SelectMany(layer => layer).Where(node => node.Value != null);
        }

        static IEnumerable<GraphEdge> GetLayeredSuccessors<TNodeValue, TEdgeLabel>(Node<TNodeValue, TEdgeLabel> node, int layer, Dictionary<Node<TNodeValue, TEdgeLabel>, GraphNode> layerMap)
        {
            foreach (var successor in node.Successors)
            {
                var layeredSuccessor = layerMap[successor.Target];
                var currentSuccessor = layeredSuccessor;

                var property = TypeDescriptor.CreateProperty(typeof(Edge<TNodeValue, TEdgeLabel>), "Label", typeof(TEdgeLabel));
                var context = new TypeDescriptorContext(successor, property);
                for (int i = layeredSuccessor.Layer + 1; i < layer; i++)
                {
                    var edge = new GraphEdge(context, successor.Label, currentSuccessor);
                    var dummyNode = new GraphNode(null, i, Enumerable.Repeat(edge, 1));
                    dummyNode.Tag = edge;
                    currentSuccessor = dummyNode;
                }

                yield return new GraphEdge(context, successor.Label, currentSuccessor);
            }
        }

        static IEnumerable<GraphNode> ComputeLongestPathLayering<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source)
        {
            var layerMap = new Dictionary<Node<TNodeValue, TEdgeLabel>, GraphNode>();
            foreach (var node in source.TopologicalSort().Reverse())
            {
                var layer = 0;
                foreach (var successor in node.Successors)
                {
                    layer = Math.Max(layer, layerMap[successor.Target].Layer);
                }

                if (node.Successors.Count > 0) layer++;
                var layeredSuccessors = GetLayeredSuccessors(node, layer, layerMap).ToList();
                foreach (var layeredSuccessor in layeredSuccessors.Where(successor => successor.Node.Value == null))
                {
                    var dummyNode = layeredSuccessor.Node;
                    while (dummyNode != null)
                    {
                        yield return dummyNode;
                        dummyNode = (from successor in dummyNode.Successors
                                     where successor.Node.Value == null
                                     select successor.Node)
                                     .FirstOrDefault();
                    }
                }

                var layeredNode = new GraphNode(node.Value, layer, layeredSuccessors);
                layeredNode.Tag = node;
                layerMap.Add(node, layeredNode);
                yield return layeredNode;
            }
        }

        public static IEnumerable<GraphNodeGrouping> LongestPathLayering<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source)
        {
            Dictionary<int, GraphNodeGrouping> layers = new Dictionary<int, GraphNodeGrouping>();
            foreach (var layeredNode in ComputeLongestPathLayering(source))
            {
                GraphNodeGrouping layer;
                if (!layers.TryGetValue(layeredNode.Layer, out layer))
                {
                    layer = new GraphNodeGrouping(layeredNode.Layer);
                    layers.Add(layer.Key, layer);
                }

                layer.Insert(0, layeredNode);
            }

            return layers.Values;
        }

        static int[] RadixSortEdges(GraphNodeGrouping northern, bool flip)
        {
            if (flip)
            {
                return (from south in northern
                        from north in south.Successors
                        orderby north.Node.LayerIndex ascending, south.LayerIndex ascending
                        select south.LayerIndex).ToArray();
            }
            else
            {
                return (from north in northern
                        from south in north.Successors
                        orderby north.LayerIndex ascending, south.Node.LayerIndex ascending
                        select south.Node.LayerIndex).ToArray();
            }
        }

        public static int NumberOfCrossings(GraphNodeGrouping northern, GraphNodeGrouping southern)
        {
            // Barth et al. JGAA 2004
            // "Simple and Efficient Bilayer Cross Counting"
            var p = northern.Count();
            var q = southern.Count();
            var flip = q > p;

            var southsequence = RadixSortEdges(northern, flip);
            q = flip ? p : q;

            /* build the accumulator tree */
            var firstindex = 1;
            while (firstindex < q) firstindex *= 2;
            var treesize = 2 * firstindex - 1; /* number of tree nodes */
            firstindex -= 1; /* index of leftmost leaf */

            var tree = new int[treesize];
            
            /* count the crossings */
            var crosscount = 0; /* number of crossings */
            for (int k = 0; k < southsequence.Length; k++)
            { /* insert edge k */
                var index = southsequence[k] + firstindex;
                tree[index]++;
                while (index > 0)
                {
                    if (index % 2 != 0) crosscount += tree[index + 1];
                    index = (index - 1) / 2;
                    tree[index]++;
                }
            }

            return crosscount;
        }

        class LayerPriorityComparer : IComparer<string>
        {
            public static readonly IComparer<string> Default = new LayerPriorityComparer();

            public int Compare(string x, string y)
            {
                var nullX = string.IsNullOrEmpty(x);
                var nullY = string.IsNullOrEmpty(y);
                if (nullX || nullY || x.Length != y.Length) return 0;
                return x.CompareTo(y);
            }
        }

        public static IEnumerable<GraphNodeGrouping> EnsureEdgeLabelPriority(this IEnumerable<GraphNodeGrouping> source)
        {
            GraphNodeGrouping successorLayer = null;
            var implicitOrder = new List<GraphEdge>();
            var partialOrder = new DirectedGraph<GraphNode, int>();
            var predecessorNodes = new List<Node<GraphNode, int>>();
            var result = new List<GraphNodeGrouping>();
            foreach (var layer in source)
            {
                var predecessorLayer = layer;
                if (successorLayer != null)
                {
                    predecessorNodes.AddRange(predecessorLayer.Select(node => new Node<GraphNode, int>(node)));
                    var ordering = from node in successorLayer
                                   select (from predecessor in predecessorNodes
                                           from edge in predecessor.Value.Successors
                                           where edge.Node == node
                                           orderby predecessor.Value.BuildDependency, edge.Text ascending
                                           select new { predecessor, edge });
                    foreach (var group in ordering)
                    {
                        Node<GraphNode, int> previous = null;
                        foreach (var link in group)
                        {
                            partialOrder.Add(link.predecessor);
                            if (previous != null) partialOrder.AddEdge(previous, link.predecessor, 0);
                            implicitOrder.Add(link.edge);
                            previous = link.predecessor;
                        }
                    }

                    // Compute partial ordering of all predecessor nodes
                    var orderedLayer = new GraphNodeGrouping(successorLayer.Key + 1);
                    foreach (var node in partialOrder.TopologicalSort())
                    {
                        orderedLayer.Add(node.Value);
                    }

                    // If no compatible partial ordering was found, we need to introduce a dummy layer
                    if (orderedLayer.Count == 0)
                    {
                        var dummyLayer = orderedLayer;
                        orderedLayer = new GraphNodeGrouping(dummyLayer.Key + 1);
                        foreach (var link in implicitOrder)
                        {
                            var edge = new GraphEdge(link, link.Node);
                            var dummyNode = new GraphNode(null, dummyLayer.Key, Enumerable.Repeat(edge, 1));
                            dummyNode.Tag = edge;
                            dummyLayer.Add(dummyNode);
                            link.Node = dummyNode;
                        }

                        foreach (var node in predecessorLayer)
                        {
                            orderedLayer.Add(node);
                        }

                        result.Add(dummyLayer);
                    }

                    implicitOrder.Clear();
                    partialOrder.Clear();
                    predecessorNodes.Clear();
                    predecessorLayer = orderedLayer;
                }

                result.Add(predecessorLayer);
                successorLayer = predecessorLayer;
            }

            return result;
        }
        
        public static IEnumerable<GraphNodeGrouping> SortLayeringByConnectionKey(this IEnumerable<GraphNodeGrouping> source)
        {
            Tuple<GraphNode, string>[] ordering = null;
            var priorityMapping = new Dictionary<GraphNode, string>();
            foreach (var layer in source)
            {
                if (ordering != null)
                {
                    for (int i = 0; i < ordering.Length; i++)
                    {
                        var entry = ordering[i];
                        var successorPriority = 0;
                        var entryPriority = string.IsNullOrEmpty(entry.Item2) ? i.ToString(CultureInfo.InvariantCulture) : entry.Item2;
                        foreach (var predecessor in layer.SelectMany(node =>
                            node.Successors.Where(edge => edge.Node == entry.Item1)
                            .Select(edge => new { label = (ExpressionBuilderArgument)edge.Label, node }))
                            .OrderBy(edge => edge.label.Index))
                        {
                            priorityMapping[predecessor.node] = entryPriority + successorPriority++;
                        }
                    }
                }

                ordering = (from node in layer
                            let priority = priorityMapping.ContainsKey(node) ? priorityMapping[node] : string.Empty
                            select Tuple.Create(node, priority))
                            .OrderBy(entry => entry.Item2, LayerPriorityComparer.Default)
                            .ToArray();

                for (int i = 0; i < ordering.Length; i++)
                {
                    layer[i] = ordering[i].Item1;
                }
            }

            return source;
        }

        public static IEnumerable<GraphNodeGrouping> EnsureLayerPriority(this IEnumerable<GraphNodeGrouping> source)
        {
            var priorityMapping = new Dictionary<GraphNode, string>();
            foreach (var layer in source.Reverse())
            {
                var ordering = (from node in layer
                                let priority = priorityMapping.ContainsKey(node) ? priorityMapping[node] : string.Empty
                                select new { node, priority })
                                .OrderBy(entry => entry.priority, LayerPriorityComparer.Default)
                                .ToArray();

                int i = 0;
                foreach (var entry in ordering)
                {
                    var entryPriority = string.IsNullOrEmpty(entry.priority) ? i.ToString(CultureInfo.InvariantCulture) : entry.priority;
                    var successorPriority = 0;

                    layer[i++] = entry.node;
                    foreach (var successor in entry.node.Successors)
                    {
                        priorityMapping[successor.Node] = entryPriority + successorPriority++;
                    }
                }
            }

            return source;
        }

        public static IEnumerable<GraphNodeGrouping> AverageMinimizeCrossings(this IEnumerable<GraphNodeGrouping> source)
        {
            //TODO: Remove method side-effects by creating new layer instances
            foreach (var layer in source)
            {
                var ordering = (from node in layer
                                let average = node.Successors.Average(successor => (int?)successor.Node.LayerIndex)
                                orderby average.HasValue ? average : -1 ascending
                                select node).ToArray();

                int i = 0;
                foreach (var node in ordering)
                {
                    layer[i++] = node;
                }

                yield return layer;
            }
        }

        public static IEnumerable<GraphNodeGrouping> MinimizeCrossings(this IEnumerable<GraphNodeGrouping> source)
        {
            var layers = source.ToArray();
            for (int i = 0; i < layers.Length - 1; i++)
            {
                var layer = layers[i + 1];
                var nextLayer = layers[i];

                var minCrossings = NumberOfCrossings(layer, nextLayer);
                for (int k = 0; k < layer.Count - 1; k++)
                {
                    var node = layer[k];
                    var neighbor = layer[k + 1];
                    layer[k + 1] = node;
                    layer[k] = neighbor;

                    var crossings = NumberOfCrossings(layer, nextLayer);
                    if (crossings < minCrossings)
                    {
                        minCrossings = crossings;
                    }
                    else
                    {
                        layer[k] = node;
                        layer[k + 1] = neighbor;
                    }
                }
            }

            return layers;
        }

        public static IEnumerable<GraphNodeGrouping> SortLayerEdgeLabels(this IEnumerable<GraphNodeGrouping> source)
        {
            var layers = source.ToArray();
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                if (i > 0)
                {
                    var nodeOrder = from node in layer
                                    from edge in node.Successors
                                    orderby node.BuildDependency, edge.Node.LayerIndex, edge.Label
                                    group node by node into g
                                    select g.Key;
                    var sortedLayer = new GraphNodeGrouping(layer.Key);
                    foreach (var node in nodeOrder)
                    {
                        sortedLayer.Add(node);
                    }

                    layers[i] = sortedLayer;
                }
            }

            return layers;
        }

        class ConnectedComponent<TNodeValue, TEdgeLabel> : DirectedGraph<TNodeValue, TEdgeLabel>
        {
            public ConnectedComponent(int index)
            {
                Index = index;
            }

            public int Index { get; private set; }
        }

        static IEnumerable<ConnectedComponent<TNodeValue, TEdgeLabel>> FindConnectedComponents<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source)
        {
            var connectedComponents = new List<ConnectedComponent<TNodeValue, TEdgeLabel>>();
            var connectedComponentMap = new Dictionary<Node<TNodeValue, TEdgeLabel>, ConnectedComponent<TNodeValue, TEdgeLabel>>();
            var visited = new Queue<Node<TNodeValue, TEdgeLabel>>();
            foreach (var node in source)
            {
                ConnectedComponent<TNodeValue, TEdgeLabel> component = null;
                if (!connectedComponentMap.TryGetValue(node, out component))
                {
                    foreach (var successor in node.DepthFirstSearch())
                    {
                        ConnectedComponent<TNodeValue, TEdgeLabel> successorComponent;
                        if (connectedComponentMap.TryGetValue(successor, out successorComponent))
                        {
                            if (component != null && component != successorComponent)
                            {
                                // Merge connected components
                                foreach (var componentNode in component)
                                {
                                    successorComponent.Add(componentNode);
                                    connectedComponentMap[componentNode] = successorComponent;
                                }
                                connectedComponents.Remove(component);
                            }
                            
                            component = successorComponent;
                        }
                        else if (!visited.Contains(successor))
                        {
                            visited.Enqueue(successor);
                        }
                    }

                    if (component == null)
                    {
                        component = new ConnectedComponent<TNodeValue, TEdgeLabel>(connectedComponents.Count);
                        connectedComponents.Add(component);
                    }

                    while (visited.Count > 0)
                    {
                        var componentNode = visited.Dequeue();
                        component.Add(componentNode);
                        connectedComponentMap.Add(componentNode, component);
                    }
                }
            }

            return connectedComponents;
        }

        public static IEnumerable<GraphNodeGrouping> ConnectedComponentLayering<TNodeValue, TEdgeLabel>(this DirectedGraph<TNodeValue, TEdgeLabel> source)
        {
            int layerOffset = 0;
            List<GraphNodeGrouping> layers = new List<GraphNodeGrouping>();
            var connectedComponents = FindConnectedComponents(source);
            foreach (var component in connectedComponents)
            {
                var maxLayerCount = 0;
                var layeredComponent = component
                    .LongestPathLayering()
                    .EnsureLayerPriority()
                    .SortLayerEdgeLabels()
                    .ToList();

                foreach (var layer in layeredComponent)
                {
                    if (layer.Key < layers.Count)
                    {
                        foreach (var node in layer)
                        {
                            layers[layer.Key].Add(node);
                        }
                    }
                    else
                    {
                        layers.Add(layer);
                        layer.UpdateItems = false;
                    }

                    foreach (var node in layer)
                    {
                        node.LayerIndex += layerOffset;
                    }

                    maxLayerCount = Math.Max(maxLayerCount, layer.Count);
                }

                layerOffset += maxLayerCount;
            }

            return layers;
        }
    }
}
