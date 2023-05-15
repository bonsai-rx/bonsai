using System;
using Bonsai.Dag;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class DepthFirstSearchTests : TestGraphExtensions
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DepthFirstSearch_NullNode_ArgumentNullException()
        {
            Node<int, int> node = default;
            node.DepthFirstSearch();
        }

        [TestMethod]
        public void DepthFirstSearch_FromRootNode_ReturnsSearchOrder()
        {
            AssertOrder(new TestGraph(10)
            {
                { 'A', 'B', 'C', 'D' },
                { 'A', 'E', 'F', 'G' },
                { 'A', 'E', 'H' },
                { 'A', 'I', 'J' }
            }.DepthFirstSearch(), expected: "ABCDEFGHIJ");
        }

        [TestMethod]
        public void DepthFirstSearch_GraphWithCycles_ReturnsSearchOrder()
        {
            AssertOrder(new TestGraph(7)
            {
                { 'A', 'B', 'D' },
                { 'A', 'B', 'F', 'E', 'A' },
                { 'A', 'C', 'G' },
                { 'A', 'E' }
            }.DepthFirstSearch(), expected: "ABDFECG");
        }
    }
}
