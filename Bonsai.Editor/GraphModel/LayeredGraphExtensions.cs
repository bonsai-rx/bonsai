﻿using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Dag;
using Bonsai.Expressions;
using System.ComponentModel;

namespace Bonsai.Editor.GraphModel
{
    static class LayeredGraphExtensions
    {
        public static ExpressionBuilderGraph ToWorkflow(this IEnumerable<GraphNode> source)
        {
            return ToWorkflow(source, true);
        }

        public static ExpressionBuilderGraph ToWorkflow(this IEnumerable<GraphNode> source, bool recurse)
        {
            return source.Select(node => (Node<ExpressionBuilder, ExpressionBuilderArgument>)node.Tag)
                         .FromInspectableGraph(recurse);
        }

        public static IEnumerable<GraphNode> SortSelection(this IEnumerable<GraphNode> source, ExpressionBuilderGraph workflow)
        {
            var nodeMap = source.ToDictionary(node => node.Value);
            for (int i = 0; i < workflow.Count; i++)
            {
                if (nodeMap.TryGetValue(workflow[i].Value, out GraphNode node))
                {
                    yield return node;
                }
            }
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

        static IEnumerable<GraphNode> ComputeLongestPathLayering(this ConnectedComponent source)
        {
            var layerMap = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderArgument>, GraphNode>();
            foreach (var node in source.Reverse())
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

        public static IEnumerable<GraphNodeGrouping> LongestPathLayering(this ConnectedComponent source)
        {
            Dictionary<int, GraphNodeGrouping> layers = new Dictionary<int, GraphNodeGrouping>();
            foreach (var layeredNode in ComputeLongestPathLayering(source))
            {
                if (!layers.TryGetValue(layeredNode.Layer, out GraphNodeGrouping layer))
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
                        if (predecessorMap.TryGetValue(node, out IEnumerable<GraphEdge> nodePredecessors))
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
            public ConnectedComponent(IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> nodes)
            {
                InsertRange(0, nodes);
            }
        }

        public static IReadOnlyList<ConnectedComponent> FindConnectedComponents(this ExpressionBuilderGraph source)
        {
            var connectedComponents = new List<ConnectedComponent>();
            var connectedSet = new HashSet<Node<ExpressionBuilder, ExpressionBuilderArgument>>();
            var currentComponent = new List<Node<ExpressionBuilder, ExpressionBuilderArgument>>();
            foreach (var node in source.TopologicalSort())
            {
                currentComponent.Add(node);
                connectedSet.Remove(node);
                if (node.Successors.Count == 0 && connectedSet.Count == 0)
                {
                    // node is the last sink in the connected component
                    connectedComponents.Add(new ConnectedComponent(currentComponent));
                    currentComponent.Clear();
                }
                else
                {
                    // all the node successors are members of the connected component
                    foreach (var successor in node.Successors)
                    {
                        connectedSet.Add(successor.Target);
                    }
                }
            }

            return connectedComponents;
        }

        public static IReadOnlyList<GraphNodeGrouping> ConnectedComponentLayering(this ExpressionBuilderGraph source)
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
                    var layer = layeredComponent[0];
                    if (singletonLayer == null) singletonLayer = layer;
                    else if (!layer[0].IsAnnotation) singletonLayer.Add(layer[0]);
                    else
                    {
                        MergeSingletonComponentLayers(ref singletonLayer, layers, ref layerOffset);
                        singletonLayer = layer;
                    }
                    continue;
                }

                MergeSingletonComponentLayers(ref singletonLayer, layers, ref layerOffset);
                MergeConnectedComponentLayers(layeredComponent, layers, ref layerOffset);
            }

            MergeSingletonComponentLayers(ref singletonLayer, layers, ref layerOffset);
            SetLayeredNodeIndices(layers, source);
            return layers;
        }

        static void SetLayeredNodeIndices(IEnumerable<GraphNodeGrouping> layers, ExpressionBuilderGraph workflow)
        {
            var nodeMap = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderArgument>, int>(workflow.Count);
            for (int i = 0; i < workflow.Count; i++)
            {
                nodeMap[workflow[i]] = i;
            }

            foreach (var layer in layers)
            {
                foreach (var node in layer)
                {
                    if (node.Tag is Node<ExpressionBuilder, ExpressionBuilderArgument> tag)
                    {
                        node.Index = nodeMap[tag];
                    }
                }
            }
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
