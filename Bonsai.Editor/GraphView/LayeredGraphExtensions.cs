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

        static IEnumerable<GraphEdge> GetLayeredSuccessors(Node<ExpressionBuilder, ExpressionBuilderArgument> node, int layer, Dictionary<Node<ExpressionBuilder, ExpressionBuilderArgument>, GraphNode> layerMap)
        {
            foreach (var successor in node.Successors)
            {
                var layeredSuccessor = layerMap[successor.Target];
                var currentSuccessor = layeredSuccessor;

                var property = TypeDescriptor.CreateProperty(typeof(Edge<ExpressionBuilder, ExpressionBuilderArgument>), "Label", typeof(ExpressionBuilderArgument));
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

        static IEnumerable<GraphNode> ComputeLongestPathLayering(this ExpressionBuilderGraph source)
        {
            var layerMap = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderArgument>, GraphNode>();
            foreach (var node in source.TopologicalSort().Reverse())
            {
                var layer = 0;
                foreach (var successor in node.Successors)
                {
                    var successorTarget = layerMap[successor.Target];
                    layer = Math.Max(layer, successorTarget.Layer);
                    successorTarget.ArgumentCount++;
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

        public static IEnumerable<GraphNodeGrouping> LongestPathLayering(this ExpressionBuilderGraph source)
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
                                    orderby edge.Node.LayerIndex, edge.Label
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

        public static IEnumerable<GraphNodeGrouping> RemoveSuccessorKinks(this IEnumerable<GraphNodeGrouping> source)
        {
            var layers = source.ToArray();
            // Backward pass
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                if (i > 0)
                {
                    var sortedLayer = new GraphNodeGrouping(layer.Key);
                    foreach (var node in layer)
                    {
                        var minSuccessorLayer = node.Successors.Min(edge => edge.Node.LayerIndex);
                        while (sortedLayer.Count < minSuccessorLayer)
                        {
                            var dummyNode = new GraphNode(null, layer.Key, Enumerable.Empty<GraphEdge>());
                            sortedLayer.Add(dummyNode);
                        }

                        sortedLayer.Add(node);
                    }

                    layers[i] = sortedLayer;
                }
            }

            // Forward pass
            var predecessorMap = new Dictionary<GraphNode, IEnumerable<GraphEdge>>();
            for (int i = layers.Length - 1; i >= 0; i--)
            {
                var layer = layers[i];
                if (i < layers.Length - 1)
                {
                    var sortedLayer = new GraphNodeGrouping(layer.Key);
                    foreach (var node in layer)
                    {
                        IEnumerable<GraphEdge> nodePredecessors;
                        if (predecessorMap.TryGetValue(node, out nodePredecessors))
                        {
                            var minSuccessorLayer = nodePredecessors.Min(edge => edge.Node.LayerIndex);
                            while (sortedLayer.Count < minSuccessorLayer)
                            {
                                var dummyNode = new GraphNode(null, layer.Key, Enumerable.Empty<GraphEdge>());
                                sortedLayer.Add(dummyNode);
                            }
                        }

                        sortedLayer.Add(node);
                    }

                    layers[i] = sortedLayer;
                }

                predecessorMap.Clear();
                foreach (var group in (from node in layers[i]
                                       from successor in node.Successors
                                       group new GraphEdge(null, null, node) by successor.Node))
                {
                    predecessorMap.Add(group.Key, group);
                }
            }

            return layers;
        }

        class ConnectedComponent : ExpressionBuilderGraph
        {
            public ConnectedComponent(int index)
            {
                Index = index;
            }

            public int Index { get; private set; }
        }

        static IEnumerable<ConnectedComponent> FindConnectedComponents(this ExpressionBuilderGraph source)
        {
            var connectedComponents = new List<ConnectedComponent>();
            var connectedComponentMap = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderArgument>, ConnectedComponent>();
            var visited = new Queue<Node<ExpressionBuilder, ExpressionBuilderArgument>>();
            foreach (var node in source)
            {
                ConnectedComponent component = null;
                if (!connectedComponentMap.TryGetValue(node, out component))
                {
                    foreach (var successor in node.DepthFirstSearch())
                    {
                        ConnectedComponent successorComponent;
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
                        component = new ConnectedComponent(connectedComponents.Count);
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

        public static IEnumerable<GraphNodeGrouping> ConnectedComponentLayering(this ExpressionBuilderGraph source)
        {
            int layerOffset = 0;
            GraphNodeGrouping singletonLayer = null;
            List<GraphNodeGrouping> layers = new List<GraphNodeGrouping>();
            var connectedComponents = FindConnectedComponents(source);
            foreach (var component in connectedComponents)
            {
                var layeredComponent = component
                    .LongestPathLayering()
                    .EnsureLayerPriority()
                    .SortLayerEdgeLabels()
                    .RemoveSuccessorKinks()
                    .ToList();
                if (component.Count == 1)
                {
                    if (singletonLayer == null) singletonLayer = layeredComponent[0];
                    else
                    {
                        var layer = layeredComponent[0];
                        singletonLayer.Add(layer[0]);
                    }
                    continue;
                }

                MergeSingletonComponentLayers(ref singletonLayer, layers, ref layerOffset);
                MergeConnectedComponentLayers(layeredComponent, layers, ref layerOffset);
            }

            MergeSingletonComponentLayers(ref singletonLayer, layers, ref layerOffset);
            return layers;
        }

        static void MergeSingletonComponentLayers(ref GraphNodeGrouping singletonLayer, List<GraphNodeGrouping> layers, ref int layerOffset)
        {
            if (singletonLayer != null)
            {
                var layeredSingleton = new List<GraphNodeGrouping>();
                for (int i = 0; i < singletonLayer.Count; i++)
                {
                    var node = singletonLayer[i];
                    var group = new GraphNodeGrouping(singletonLayer.Count - i - 1);
                    node.Layer = group.Key;
                    group.Add(node);
                    layeredSingleton.Insert(0, group);
                }

                MergeConnectedComponentLayers(layeredSingleton, layers, ref layerOffset);
                singletonLayer = null;
            }
        }

        static void MergeConnectedComponentLayers(List<GraphNodeGrouping> layeredComponent, List<GraphNodeGrouping> layers, ref int layerOffset)
        {
            var maxLayerCount = 0;
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
    }
}
