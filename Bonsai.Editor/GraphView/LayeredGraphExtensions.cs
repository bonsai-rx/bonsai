using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;
using System.Globalization;
using Bonsai.Expressions;
using System.ComponentModel;

namespace Bonsai.Editor.GraphView
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

        static bool RemoveBranchKinks(GraphNodeGrouping[] layers)
        {
            // Backward pass
            var removed = false;
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = layers[i];
                if (i > 0)
                {
                    var sortedLayer = new GraphNodeGrouping(layer.Key);
                    foreach (var node in layer)
                    {
                        if (node.Successors != Enumerable.Empty<GraphEdge>())
                        {
                            var minSuccessorLayer = node.Successors.Min(edge => edge.Node.LayerIndex);
                            while (sortedLayer.Count < minSuccessorLayer)
                            {
                                var dummyNode = new GraphNode(null, layer.Key, Enumerable.Empty<GraphEdge>());
                                sortedLayer.Add(dummyNode);
                                removed = true;
                            }
                        }

                        sortedLayer.Add(node);
                    }

                    layers[i] = sortedLayer;
                }
            }

            return removed;
        }

        static bool RemoveMergeGaps(GraphNodeGrouping[] layers)
        {
            // Forward pass
            var removed = false;
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
                                removed = true;
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

            return removed;
        }

        public static IEnumerable<GraphNodeGrouping> RemoveSuccessorKinks(this IEnumerable<GraphNodeGrouping> source)
        {
            var layers = source.ToArray();
            RemoveBranchKinks(layers);
            if (RemoveMergeGaps(layers))
            {
                RemoveBranchKinks(layers);
            }
            return layers;
        }

        public class ConnectedComponent : ExpressionBuilderGraph
        {
            public ConnectedComponent(int index)
            {
                Index = index;
            }

            public int Index { get; private set; }
        }

        public static IList<ConnectedComponent> FindConnectedComponents(this ExpressionBuilderGraph source)
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
