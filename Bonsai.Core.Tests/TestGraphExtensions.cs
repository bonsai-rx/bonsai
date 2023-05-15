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
            readonly Dictionary<char, int> argumentCount;

            public TestGraph(int count)
            {
                nodes = new Dictionary<char, Node<string, int>>(count);
                argumentCount = new Dictionary<char, int>(count);
                for (int i = 0; i < count; i++)
                {
                    var key = (char)('A' + i);
                    nodes.Add(key, Add(key.ToString()));
                    argumentCount.Add(key, 0);
                }
            }

            public void Add(char from, char to)
            {
                AddEdge(nodes[from], nodes[to], argumentCount[to]++);
            }

            public void Add(int label, char from, char to)
            {
                AddEdge(nodes[from], nodes[to], label);
            }

            public void Add(params char[] chain)
            {
                for (int i = 1; i < chain.Length; i++)
                {
                    Add(chain[i - 1], chain[i]);
                }
            }

            public void Add(int label, params char[] chain)
            {
                for (int i = 1; i < chain.Length; i++)
                {
                    Add(label, chain[i - 1], chain[i]);
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
