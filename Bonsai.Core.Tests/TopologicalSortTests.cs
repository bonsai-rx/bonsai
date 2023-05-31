using Bonsai.Dag;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class TopologicalSortTests : TestGraphExtensions
    {
        static void AssertOrder(TestGraph graph, string expected)
        {
            var order = graph.TopologicalSort();
            AssertOrder(order, expected);

            var orderedGraph = new DirectedGraph<string, int>();
            orderedGraph.InsertRange(0, order);
            var secondOrder = orderedGraph.TopologicalSort();
            AssertOrder(secondOrder, expected);
        }

        [TestMethod]
        public void TopologicalSort_SimpleChain_InsertionOrder()
        {
            AssertOrder(new TestGraph(3)
            {
                { 'A', 'B', 'C' }
            }, "ABC");
        }

        [TestMethod]
        public void TopologicalSort_DisjointChains_InsertionOrder()
        {
            AssertOrder(new TestGraph(4)
            {
                { 'A', 'B' },
                { 'C', 'D' },
            }, "ABCD");
        }

        [TestMethod]
        public void TopologicalSort_SimpleBranch_InsertionOrder()
        {
            AssertOrder(new TestGraph(5)
            {
                { 'A', 'B', 'C' },
                { 'A', 'D', 'E' },
            }, "ABCDE");
        }

        [TestMethod]
        public void TopologicalSort_SimpleBranch_ScrambledOrder()
        {
            AssertOrder(new TestGraph(5)
            {
                { 'E', 'C', 'D' },
                { 'E', 'A', 'B' },
            }, "EABCD");
        }

        [TestMethod]
        public void TopologicalSort_NestedBranch_InsertionOrder()
        {
            AssertOrder(new TestGraph(6)
            {
                { 'A', 'B', 'C' },
                     { 'B', 'D' },
                { 'A', 'E', 'F' },
            }, "ABCDEF");
        }

        [TestMethod]
        public void TopologicalSort_NestedBranch_ScrambledOrder()
        {
            AssertOrder(new TestGraph(6)
            {
                { 'F', 'B', 'E' },
                     { 'B', 'A' },
                { 'F', 'C', 'D' },
            }, "FBACDE");
        }

        [TestMethod]
        public void TopologicalSort_SimpleMerge_InsertionOrder()
        {
            AssertOrder(new TestGraph(4)
            {
                { 'A', 'B', 'C' },
                { 'D', 'B' },
            }, "ADBC");
        }

        [TestMethod]
        public void TopologicalSort_DeferredMerge_InsertionOrder()
        {
            AssertOrder(new TestGraph(4)
            {
                { 'A', 'B', 'C' },
                { 'D', 'A' },
                { 'D', 'B' }
            }, "DABC");
        }

        [TestMethod]
        public void TopologicalSort_MergeBranch_ScrambledOrder()
        {
            AssertOrder(new TestGraph(5)
            {
                { 'C', 'D', 'E', 'B' },
                          { 'E', 'A', 'B' },
            }, "CDEAB");
        }

        [TestMethod]
        public void TopologicalSort_MergeLongBranch_InsertionOrder()
        {
            AssertOrder(new TestGraph(5)
            {
                { 'A',           'B', 'C' },
                { 'A', 'D', 'E', 'B' }
            }, "ADEBC");
        }

        [TestMethod]
        public void TopologicalSort_MergeExternalBranch_InsertionOrder()
        {
            AssertOrder(new TestGraph(8)
            {
                { 'A', 'B', 'C', 'D' },
                          { 'C', 'E' },
                { 'A', 'F', 'G' },
                { 'H', 'F' }
            }, "ABCDEHFG");
        }

        [TestMethod]
        public void TopologicalSort_MergeDanglingBranch_InsertionOrder()
        {
            AssertOrder(new TestGraph(4)
            {
                { 'A', 'B', 'C' },
                { 'A',      'C' },
                {      'B', 'D' }
            }, "ABCD");
        }

        [TestMethod]
        public void TopologicalSort_MergeDanglingSource_InsertionOrder()
        {
            AssertOrder(new TestGraph(8)
            {
                { 'F',      'A' },
                { 'F', 'C', 'B' },
                { 'G', 'C' },
                { 'H', 'B' },
                { 'D', 'E' }
            }, "FAGCHBDE");
        }

        [TestMethod]
        public void TopologicalSort_ConnectedComponents_InsertionOrder()
        {
            AssertOrder(new TestGraph(6)
            {
                { 'A', 'C' },
                { 'F', 'B' },
                { 'D', 'E' }
            }, "FBACDE");
        }

        [TestMethod]
        public void TopologicalSort_DeferredConnectedComponents_ConnectedComponentOrder()
        {
            AssertOrder(new TestGraph(7)
            {
                { 'A', 'B' },
                { 'C', 'D' },
                { 'E', 'F' },
                { 'G', 'E' },
                { 'G', 'A' }
            }, "GABEFCD");
        }

        [TestMethod]
        public void TopologicalSort_ScrambledConnectedComponentConnections_SingleComponent()
        {
            AssertOrder(new TestGraph(12)
            {
                { 'A', 'B' },
                { 'C', 'D' },
                { 'E', 'F' },
                { 'G', 'H' },
                { 'I', 'J' },
                { 'K', 'C' },
                { 'K', 'E' },
                { 'L', 'G' },
                { 'L', 'K' },
                { 'L', 'I' },
                { 'L', 'A' },
            }, "LABKCDEFGHIJ");
        }

        [TestMethod]
        public void TopologicalSort_DanglingBranchFromMergeInput_InsertionOrder()
        {
            AssertOrder(new TestGraph(5)
            {
                { 'A', 'D' },
                { 'B', 'D' },
                { 'C', 'D' },
                { 'B', 'E' }
            }, "ABCDE");
        }

        [TestMethod]
        public void TopologicalSort_DanglingBranchFromMergeInput_ScrambledOrder()
        {
            AssertOrder(new TestGraph(5)
            {
                { 'D', 'E', 'A' },
                     { 'E', 'B', 'A' },
                     { 'C', 'A' }
            }, "DEBCA");
        }

        [TestMethod]
        public void TopologicalSort_BranchToMergeInputScrambledConstructor_ScrambledOrder()
        {
            AssertOrder(new TestGraph(5)
            {
                { 'D', 'E', 'A' },
                     { 'E', 'B', 'A' },
                { 'D', 'C', 'A' }
            }, "DEBCA");
        }

        [TestMethod]
        public void TopologicalSort_SelfDependencyReference_InsertionOrder()
        {
            AssertOrder(new TestGraph(4)
            {
                { 'B',           'A' },
                { 'B', 'C',      'A' },
                     { 'C', 'D', 'A' },
            }, "BCDA");
        }

        [TestMethod]
        public void TopologicalSort_CyclicGraph_ReturnsEmptySequence()
        {
            AssertOrder(new TestGraph(4)
            {
                { 'B',           'A' },
                { 'B', 'C',      'A' },
                     { 'C', 'D', 'B' },
            }, string.Empty);
        }

        [TestMethod]
        public void TopologicalSort_CrossedDependencyRoots_ConnectedComponentOrder()
        {
            AssertOrder(new TestGraph(16)
            {
                { 'A', 'B', 'D' },
                {      'C', 'D' },
                {      'E', 'F', 'K', 'L' },
                {      'B', 'I', 'J', 'K' },
                {           'F', 'M', 'N', 'P' },
                {           'G', 'H', 'O', 'P' },
            }, "ABCDEFIJKLMNGHOP");
        }
    }
}
