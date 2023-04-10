using System.Collections.Generic;
using System.Linq;
using Bonsai.Dag;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    public class TestGraphExtensions
    {
        protected class TestGraph : DirectedGraph<string, int>
        {
            readonly Dictionary<char, Node<string, int>> nodes;

            public TestGraph(int count)
            {
                nodes = new Dictionary<char, Node<string, int>>(count);
                for (int i = 0; i < count; i++)
                {
                    var key = (char)('A' + i);
                    nodes.Add(key, Add(key.ToString()));
                }
            }

            public void Add(char from, char to)
            {
                AddEdge(nodes[from], nodes[to], 0);
            }

            public void Add(int label, char from, char to)
            {
                AddEdge(nodes[from], nodes[to], label);
            }

            public void Add(params char[] chain)
            {
                Add(0, chain);
            }

            public void Add(int label, params char[] chain)
            {
                for (int i = 1; i < chain.Length; i++)
                {
                    AddEdge(nodes[chain[i - 1]], nodes[chain[i]], label);
                }
            }
        }

        protected static void AssertOrder(IEnumerable<Node<string, int>> order, string expected)
        {
            var actual = string.Concat(order.Select(node => node.Value));
            Assert.AreEqual(expected, actual);
        }
    }
}
