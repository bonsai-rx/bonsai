using Bonsai.Dag;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class TopologicalSortTests
    {
        class TestGraph : IEnumerable<Node<string, int>>
        {
            readonly Dictionary<char, Node<string, int>> nodes;
            readonly DirectedGraph<string, int> graph;

            public TestGraph(int count)
            {
                nodes = new Dictionary<char, Node<string, int>>(count);
                graph = new DirectedGraph<string, int>();
                for (int i = 0; i < count; i++)
                {
                    var key = (char)('A' + i);
                    nodes.Add(key, graph.Add(key.ToString()));
                }
            }

            public void Add(char from, char to)
            {
                graph.AddEdge(nodes[from], nodes[to], 0);
            }

            public void Add(int label, char from, char to)
            {
                graph.AddEdge(nodes[from], nodes[to], label);
            }

            public void Add(params char[] chain)
            {
                Add(0, chain);
            }

            public void Add(int label, params char[] chain)
            {
                for (int i = 1; i < chain.Length; i++)
                {
                    graph.AddEdge(nodes[chain[i - 1]], nodes[chain[i]], label);
                }
            }

            public IEnumerable<Node<string, int>> TopologicalSort()
            {
                return graph.TopologicalSort();
            }

            public IEnumerator<Node<string, int>> GetEnumerator()
            {
                return graph.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        static void AssertOrder(IEnumerable<Node<string, int>> order, string expected)
        {
            var actual = string.Concat(order.Select(node => node.Value));
            Assert.AreEqual(expected, actual);
        }

        static void AssertOrder(TestGraph graph, string expected)
        {
            var order = graph.TopologicalSort();
            AssertOrder(order, expected);

            var orderedGraph = new DirectedGraph<string, int>();
            foreach (var node in order) orderedGraph.Add(node);
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
            }, "ECDAB");
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
            }, "FBEACD");
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
            // C comes after D, because C has a dependency on
            // the second branch of A
            AssertOrder(new TestGraph(4)
            {
                { 'A', 'B', 'C' },
                { 'A',      'C' },
                {      'B', 'D' }
            }, "ABDC");
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
            }, "ACFBDE");
        }

        [TestMethod]
        public void TopologicalSort_DeferredConnectedComponents_InsertionOrder()
        {
            AssertOrder(new TestGraph(7)
            {
                { 'A', 'B' },
                { 'C', 'D' },
                { 'E', 'F' },
                { 'G', 'E' },
                { 'G', 'A' }
            }, "CDGEFAB");
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
            }, "LGHKCDEFIJAB");
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
            }, "CDEBA");
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
    }
}
