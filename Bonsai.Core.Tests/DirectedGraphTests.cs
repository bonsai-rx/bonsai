using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Dag;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class DirectedGraphTests
    {
        static DirectedGraph<int, int> CreateGraph(params int[] values)
        {
            var graph = new DirectedGraph<int, int>();
            for (int i = 0; i < values.Length; i++)
            {
                graph.Add(values[i]);
            }
            return graph;
        }

        static string FormatNodeSequence<TNodeValue, TEdgeLabel>(IEnumerable<Node<TNodeValue, TEdgeLabel>> source)
        {
            return FormatSequence(source.Select(node => node.Value));
        }

        static string FormatSequence<TSource>(IEnumerable<TSource> source)
        {
            return string.Join(ExpressionHelper.ArgumentSeparator, source);
        }

        #region Add

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullNode_ArgumentNullException()
        {
            var graph = CreateGraph();
            graph.Add(null);
        }

        [TestMethod]
        public void Add_NodeValue_NodeReturnsValue()
        {
            const int NodeValue = 11;
            var graph = CreateGraph(NodeValue);
            Assert.AreEqual(expected: 1, graph.Count);
            Assert.AreEqual(NodeValue, graph[0].Value);
        }

        [TestMethod]
        public void Add_DuplicateNode_GraphCountReturnsUniqueCount()
        {
            var graph = CreateGraph(0);
            Assert.AreEqual(expected: 1, graph.Count);
            graph.Add(graph[0]);
            Assert.AreEqual(expected: 1, graph.Count);
        }

        [TestMethod]
        public void Add_NodeWithSuccessors_GraphCountReturnsSuccessorCount()
        {
            var graph = CreateGraph();
            var node = new Node<int, int>(0);
            var successor = new Node<int, int>(1);
            node.Successors.Add(Edge.Create(successor, label: 0));
            Assert.AreEqual(expected: 0, graph.Count);
            graph.Add(node);
            Assert.AreEqual(expected: 2, graph.Count);
        }

        #endregion

        #region AddEdge

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddEdge_NullSourceNode_ArgumentNullException()
        {
            var graph = CreateGraph(0);
            graph.AddEdge(from: null, to: graph[0], label: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddEdge_NullTargetNode_ArgumentNullException()
        {
            var graph = CreateGraph(0);
            graph.AddEdge(from: graph[0], to: null, label: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddEdge_NullEdge_ArgumentNullException()
        {
            var graph = CreateGraph(0);
            graph.AddEdge(from: graph[0], edge: null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddEdge_SourceNodeNotInGraph_ArgumentException()
        {
            var graph = CreateGraph(0);
            var node = new Node<int, int>(1);
            graph.AddEdge(from: node, to: graph[0], label: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddEdge_TargetNodeNotInGraph_ArgumentException()
        {
            var graph = CreateGraph(0);
            var node = new Node<int, int>(1);
            graph.AddEdge(from: graph[0], to: node, label: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddEdge_EdgeTargetNotInGraph_ArgumentException()
        {
            var graph = CreateGraph(0);
            var node = new Node<int, int>(1);
            var edge = Edge.Create(node, label: 0);
            graph.AddEdge(from: graph[0], edge);
        }

        [TestMethod]
        public void AddEdge_LabelValue_EdgeReturnsLabel()
        {
            const int EdgeLabel = 23;
            var graph = CreateGraph(0, 1);
            var edge = graph.AddEdge(graph[0], graph[1], EdgeLabel);
            Assert.AreEqual(EdgeLabel, edge.Label);
        }

        [TestMethod]
        public void AddEdge_EdgeTarget_NodeSuccessorsContainsTarget()
        {
            var graph = CreateGraph(0, 1);
            var node = graph[0];
            var edge = Edge.Create(target: graph[1], label: 0);
            graph.AddEdge(node, edge);
            Assert.IsTrue(node.Successors.Contains(edge));
        }

        #endregion

        #region InsertEdge

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InsertEdge_NullEdge_ArgumentNullException()
        {
            var graph = CreateGraph(0);
            graph.InsertEdge(from: graph[0], edgeIndex: 0, edge: null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InsertEdge_NegativeIndex_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0, 1);
            graph.InsertEdge(graph[0], edgeIndex: -1, graph[1], label: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InsertEdge_IndexOutOfRange_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0, 1);
            graph.InsertEdge(graph[0], edgeIndex: 1, graph[1], label: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InsertEdge_NegativeIndexEdgeOverload_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0, 1);
            var edge = Edge.Create(target: graph[1], label: 0);
            graph.InsertEdge(graph[0], edgeIndex: -1, edge);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InsertEdge_IndexOutOfRangeEdgeOverload_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0, 1);
            var edge = Edge.Create(target: graph[1], label: 0);
            graph.InsertEdge(graph[0], edgeIndex: 1, edge);
        }

        [TestMethod]
        public void InsertEdge_LabelValue_EdgeReturnsLabel()
        {
            const int EdgeLabel = 23;
            var graph = CreateGraph(0, 1);
            var edge = graph.InsertEdge(graph[0], edgeIndex: 0, graph[1], EdgeLabel);
            Assert.AreEqual(EdgeLabel, edge.Label);
        }

        [TestMethod]
        public void InsertEdge_EdgeTarget_IndexOfTargetEqualsEdgeIndex()
        {
            var graph = CreateGraph(0, 1);
            var node = graph[0];
            var edge1 = Edge.Create(target: graph[1], label: 0);
            var edge2 = Edge.Create(target: graph[1], label: 1);
            graph.AddEdge(node, edge1);
            graph.InsertEdge(node, edgeIndex: 0, edge2);
            Assert.IsTrue(node.Successors.Contains(edge1));
            Assert.AreEqual(expected: 0, node.Successors.IndexOf(edge2));
        }

        #endregion

        #region SetEdge

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetEdge_NullEdge_ArgumentNullException()
        {
            var graph = CreateGraph(0);
            graph.SetEdge(from: graph[0], edgeIndex: 0, edge: null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetEdge_NegativeIndex_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0, 1);
            graph.SetEdge(graph[0], edgeIndex: -1, graph[1], label: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetEdge_IndexOutOfRange_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0, 1);
            graph.SetEdge(graph[0], edgeIndex: 0, graph[1], label: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetEdge_NegativeIndexEdgeOverload_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0, 1);
            var edge = Edge.Create(target: graph[1], label: 0);
            graph.SetEdge(graph[0], edgeIndex: -1, edge);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SetEdge_IndexOutOfRangeEdgeOverload_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0, 1);
            var edge = Edge.Create(target: graph[1], label: 0);
            graph.SetEdge(graph[0], edgeIndex: 0, edge);
        }

        [TestMethod]
        public void SetEdge_LabelValue_EdgeReturnsLabel()
        {
            const int EdgeLabel = 23;
            var graph = CreateGraph(0, 1);
            var edge = graph.AddEdge(graph[0], graph[1], EdgeLabel);
            Assert.AreEqual(EdgeLabel, edge.Label);
            edge = graph.SetEdge(graph[0], edgeIndex: 0, graph[1], label: 0);
            Assert.AreEqual(expected: 0, edge.Label);
        }

        [TestMethod]
        public void SetEdge_EdgeTarget_NodeSuccessorsContainsNewTarget()
        {
            var graph = CreateGraph(0, 1);
            var node = graph[0];
            var edge1 = Edge.Create(target: graph[1], label: 0);
            var edge2 = Edge.Create(target: graph[1], label: 1);
            graph.AddEdge(node, edge1);
            graph.SetEdge(node, edgeIndex: 0, edge2);
            Assert.IsFalse(node.Successors.Contains(edge1));
            Assert.AreEqual(expected: 0, node.Successors.IndexOf(edge2));
        }

        #endregion

        #region Contains

        [TestMethod]
        public void Contains_EmptyGraph_ReturnsFalse()
        {
            var graph = CreateGraph();
            var node = new Node<int, int>(0);
            Assert.IsFalse(graph.Contains(node));
        }

        [TestMethod]
        public void Contains_AddNodeResult_ReturnsTrue()
        {
            var graph = CreateGraph();
            var node = graph.Add(0);
            Assert.IsTrue(graph.Contains(node));
        }

        #endregion

        #region IndexOf

        [TestMethod]
        public void IndexOf_NullNode_ReturnsNegativeOne()
        {
            var graph = CreateGraph();
            Assert.AreEqual(expected: -1, graph.IndexOf(null));
        }

        [TestMethod]
        public void IndexOf_InsertedNode_ReturnsSpecifiedIndex()
        {
            var graph = CreateGraph(0, 1);
            var node = graph.Insert(index: 1, value: 2);
            Assert.AreEqual(expected: 1, graph.IndexOf(node));
        }

        #endregion

        #region Insert

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Insert_NullNode_ArgumentNullException()
        {
            var graph = CreateGraph();
            graph.Insert(index: 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Insert_NegativeIndex_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0);
            graph.Insert(index: -1, graph[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Insert_NegativeIndexNodeValue_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph();
            graph.Insert(index: -1, value: 0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Insert_IndexOutOfRange_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0);
            graph.Insert(index: 2, graph[0]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Insert_IndexOutOfRangeNodeValue_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph();
            graph.Insert(index: 1, value: 0);
        }

        [TestMethod]
        public void Insert_NodeValue_NodeReturnsValue()
        {
            const int NodeValue = 11;
            var graph = CreateGraph(0, 1);
            graph.Insert(0, NodeValue);
            Assert.AreEqual(expected: 3, graph.Count);
            Assert.AreEqual(NodeValue, graph[0].Value);
        }

        [TestMethod]
        public void Insert_NodeWithSuccessors_IndexOfNodesMatchesInsertionIndex()
        {
            var graph = CreateGraph(2, 3);
            var node = new Node<int, int>(0);
            var successor = new Node<int, int>(1);
            node.Successors.Add(Edge.Create(successor, label: 0));
            Assert.AreEqual(expected: 2, graph.Count);
            graph.Insert(1, node);
            Assert.AreEqual(expected: 4, graph.Count);
            Assert.AreEqual(expected: 1, graph.IndexOf(node));
            Assert.AreEqual(expected: 2, graph.IndexOf(successor));
        }

        [TestMethod]
        public void Insert_NodeWithExistingSuccessors_ReorderNodeIndex()
        {
            var graph = CreateGraph(0, 1, 2, 3, 4, 5);
            graph.AddEdge(graph[4], graph[0], label: 0);
            graph.AddEdge(graph[0], graph[1], label: 0);
            graph.AddEdge(graph[0], graph[2], label: 1);
            Assert.AreEqual(expected: 4, graph.IndexOf(graph[4]));
            graph.Insert(3, node: graph[4]);
            Assert.AreEqual(
                FormatSequence(new[] { 0, 1, 2, 4, 3, 5 }),
                FormatNodeSequence(graph));
        }

        #endregion

        #region InsertRange

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InsertRange_NullCollection_ArgumentNullException()
        {
            var graph = CreateGraph();
            graph.InsertRange(index: 0, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InsertRange_NegativeIndex_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0);
            graph.InsertRange(index: -1, new[] { graph[0] });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void InsertRange_IndexOutOfRange_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0);
            graph.InsertRange(index: 2, new[] { graph[0] });
        }

        [TestMethod]
        public void InsertRange_ExistingNodeCollection_ReorderNodeIndices()
        {
            var graph = CreateGraph(0, 1, 2, 3, 4, 5);
            graph.AddEdge(graph[4], graph[0], label: 0);
            graph.AddEdge(graph[0], graph[2], label: 0);
            graph.AddEdge(graph[2], graph[5], label: 0);
            graph.AddEdge(graph[5], graph[4], label: 0);
            Assert.AreEqual(expected: 4, graph.IndexOf(graph[4]));
            graph.InsertRange(1, new[] { graph[2], graph[4], graph[0] });
            Assert.AreEqual(
                FormatSequence(new[] { 2, 4, 0, 1, 3, 5 }),
                FormatNodeSequence(graph));
        }

        [TestMethod]
        public void InsertRange_NodeCollectionWithSuccessors_SuccessorNodesInsertedAtEnd()
        {
            var graph = CreateGraph(2, 3);
            var node = new Node<int, int>(0);
            var node2 = new Node<int, int>(4);
            var successor = new Node<int, int>(1);
            node.Successors.Add(Edge.Create(successor, label: 0));
            Assert.AreEqual(expected: 2, graph.Count);
            graph.InsertRange(1, new[] { node, node2 });
            Assert.AreEqual(
                FormatSequence(new[] { 2, 0, 4, 1, 3 }),
                FormatNodeSequence(graph));
        }

        #endregion

        #region Remove

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Remove_NullNode_ArgumentNullException()
        {
            var graph = CreateGraph();
            graph.Remove(null);
        }

        [TestMethod]
        public void Remove_EmptyGraph_ReturnsFalse()
        {
            var graph = CreateGraph();
            var node = new Node<int, int>(0);
            Assert.IsFalse(graph.Remove(node));
        }

        [TestMethod]
        public void Remove_AddNodeResult_ReturnsTrue()
        {
            var graph = CreateGraph();
            var node = graph.Add(0);
            Assert.IsTrue(graph.Contains(node));
            Assert.IsTrue(graph.Remove(node));
            Assert.IsFalse(graph.Contains(node));
        }

        [TestMethod]
        public void Remove_NodeWithExistingLinks_ReturnsTrueAndRemovesLinks()
        {
            var graph = CreateGraph(0, 1, 2);
            var node = graph[1];
            graph.AddEdge(graph[0], node, label: 0);
            Assert.AreEqual(expected: 1, graph[0].Successors.Count);
            Assert.IsTrue(graph.Remove(node));
            Assert.AreEqual(expected: 0, graph[0].Successors.Count);
        }

        #endregion

        #region RemoveAt

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RemoveAt_NegativeIndex_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0);
            graph.RemoveAt(index: -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RemoveAt_IndexOutOfRange_ArgumentOutOfRangeException()
        {
            var graph = CreateGraph(0);
            graph.RemoveAt(index: 1);
        }

        [TestMethod]
        public void RemoveAt_NodeAtValidIndex_ReorderIndexOfExistingNodes()
        {
            var graph = CreateGraph(0, 1, 2);
            graph.AddEdge(graph[0], graph[1], label: 0);
            Assert.AreEqual(expected: 1, graph[1].Value);
            Assert.AreEqual(expected: 1, graph[0].Successors.Count);
            graph.RemoveAt(1);
            Assert.AreEqual(expected: 2, graph[1].Value);
            Assert.AreEqual(expected: 0, graph[0].Successors.Count);
        }

        #endregion

        #region RemoveEdge

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveEdge_NullEdge_ArgumentNullException()
        {
            var graph = CreateGraph(0);
            graph.RemoveEdge(graph[0], null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RemoveEdge_EdgeTargetNotInGraph_ArgumentException()
        {
            var graph = CreateGraph(0);
            var node = new Node<int, int>(1);
            var edge = Edge.Create(node, label: 0);
            graph.RemoveEdge(from: graph[0], edge);
        }

        [TestMethod]
        public void RemoveEdge_EdgeNotInGraph_ReturnsFalse()
        {
            var graph = CreateGraph(0);
            var node = graph.Add(1);
            var edge = Edge.Create(node, label: 0);
            Assert.IsFalse(graph.RemoveEdge(from: graph[0], edge));
        }

        #endregion

        #region Clear

        [TestMethod]
        public void Clear_NonEmptyGraph_CountReturnsZero()
        {
            var graph = CreateGraph(0, 1);
            Assert.IsTrue(graph.Count > 0);
            graph.Clear();
            Assert.IsTrue(graph.Count == 0);
        }

        #endregion

        #region CopyTo

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CopyTo_NullArray_ArgumentNullException()
        {
            var graph = CreateGraph(0);
            graph.CopyTo(null);
        }

        [TestMethod]
        public void CopyTo_NewArray_ArrayContainsAllNodes()
        {
            var graph = CreateGraph(0, 1, 2);
            var array = new Node<int, int>[graph.Count];
            graph.CopyTo(array);
            Assert.AreEqual(
                FormatNodeSequence(graph),
                FormatNodeSequence(array));
        }

        [TestMethod]
        public void CopyTo_ExistingArrayIndex_ArraySegmentContainsAllNodes()
        {
            var graph = CreateGraph(0, 1, 2);
            var array = new Node<int, int>[graph.Count * 2];
            graph.CopyTo(array, graph.Count);
            Assert.AreEqual(
                FormatNodeSequence(graph),
                FormatNodeSequence(array.Skip(graph.Count)));
        }

        #endregion

        #region Comparer

        [Obsolete]
        [TestMethod]
        public void GetEnumerator_WithValueComparer_ReturnsValueSortedSequence()
        {
            var graph = new DirectedGraph<int, int>(Comparer<int>.Default) { 5, 4, 1, 3, 2 };
            Assert.IsNotNull(graph.Comparer);
            Assert.AreEqual(
                FormatSequence(new[] { 1, 2, 3, 4, 5 }),
                FormatNodeSequence(graph));
        }

        #endregion

        #region ICollection

        [TestMethod]
        public void IsReadOnly_ReturnsFalse()
        {
            var graph = CreateGraph();
            Assert.IsFalse(((ICollection<Node<int, int>>)graph).IsReadOnly);
        }

        [TestMethod]
        public void GetEnumerator_NonGenericIEnumerable_ReturnsInsertedNodes()
        {
            var values = new[] { 0, 1 };
            var graph = CreateGraph(values);
            var enumerator = ((System.Collections.IEnumerable)graph).GetEnumerator();
            for (int i = 0; i < values.Length; i++)
            {
                enumerator.MoveNext();
                Assert.AreEqual(values[i], ((Node<int, int>)enumerator.Current).Value);
            }
        }

        #endregion
    }
}
