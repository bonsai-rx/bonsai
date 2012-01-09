using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Dag
{
    public class DirectedGraph<TValue, TLabel> : IEnumerable<Node<TValue, TLabel>>
    {
        readonly HashSet<Node<TValue, TLabel>> nodes = new HashSet<Node<TValue, TLabel>>();

        public int Count
        {
            get { return nodes.Count; }
        }

        public void Add(Node<TValue, TLabel> node)
        {
            nodes.Add(node);
        }

        public void AddEdge(Node<TValue, TLabel> from, Node<TValue, TLabel> to, TLabel label)
        {
            if (!nodes.Contains(from))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", "from");
            }

            if (!nodes.Contains(to))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", "to");
            }

            var edge = new Edge<TValue, TLabel>(label, to);
            from.Successors.Add(edge);
        }

        public bool Contains(Node<TValue, TLabel> node)
        {
            return nodes.Contains(node);
        }

        public bool Remove(Node<TValue, TLabel> node)
        {
            if (!nodes.Contains(node))
            {
                return false;
            }

            foreach (var n in nodes)
            {
                if (n == node) continue;
                for (int i = 0; i < n.Successors.Count; i++)
                {
                    if (n.Successors[i].Node == node)
                    {
                        n.Successors.RemoveAt(i);
                    }
                }
            }

            return nodes.Remove(node);
        }

        public bool RemoveEdge(Node<TValue, TLabel> from, Node<TValue, TLabel> to, TLabel label)
        {
            if (!nodes.Contains(from))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", "from");
            }

            if (!nodes.Contains(to))
            {
                throw new ArgumentException("The specified node does not belong to the graph.", "to");
            }

            var edge = new Edge<TValue, TLabel>(label, to);
            return from.Successors.Remove(edge);
        }

        public void Clear()
        {
            nodes.Clear();
        }

        public IEnumerator<Node<TValue, TLabel>> GetEnumerator()
        {
            return nodes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return nodes.GetEnumerator();
        }
    }
}
