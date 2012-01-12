using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Dag;

namespace Bonsai.Design
{
    public static class LayeredGraphExtensions
    {
        static IEnumerable<GraphEdge> GetLayeredSuccessors<TValue, TLabel>(Node<TValue, TLabel> node, int layer, Dictionary<Node<TValue, TLabel>, GraphNode> layerMap)
        {
            foreach (var successor in node.Successors)
            {
                var layeredSuccessor = layerMap[successor.Node];
                var currentSuccessor = layeredSuccessor;

                for (int i = layeredSuccessor.Layer + 1; i < layer; i++)
                {
                    var edge = new GraphEdge(successor.Label, currentSuccessor);
                    var dummyNode = new GraphNode(null, i, Enumerable.Repeat(edge, 1));
                    dummyNode.Tag = edge;
                    currentSuccessor = dummyNode;
                }

                yield return new GraphEdge(successor.Label, currentSuccessor);
            }
        }

        static IEnumerable<GraphNode> ComputeLongestPathLayering<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source)
        {
            var layerMap = new Dictionary<Node<TValue, TLabel>, GraphNode>();
            foreach (var node in source.TopologicalSort().Reverse())
            {
                var layer = 0;
                foreach (var successor in node.Successors)
                {
                    layer = Math.Max(layer, layerMap[successor.Node].Layer);
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

        public static IEnumerable<GraphNodeGrouping> LongestPathLayering<TValue, TLabel>(this DirectedGraph<TValue, TLabel> source)
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

                layer.Add(layeredNode);
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
                    var tmp = node.LayerIndex;
                    layer[neighbor.LayerIndex] = node;
                    layer[tmp] = neighbor;

                    var crossings = NumberOfCrossings(layer, nextLayer);
                    if (crossings < minCrossings)
                    {
                        minCrossings = crossings;
                    }
                    else
                    {
                        tmp = node.LayerIndex;
                        layer[neighbor.LayerIndex] = node;
                        layer[tmp] = neighbor;
                    }
                }
            }

            return layers;
        }
    }
}
