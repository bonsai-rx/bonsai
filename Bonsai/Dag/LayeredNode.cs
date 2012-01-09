using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Dag
{
    public class LayeredNode<TValue, TLabel>
    {
        public LayeredNode(Node<TValue, TLabel> node, int layer, IEnumerable<LayeredNode<TValue, TLabel>> successors)
        {
            Node = node;
            Layer = layer;
            Successors = successors;
        }

        public int Layer { get; private set; }

        public int LayerIndex { get; internal set; }

        public Node<TValue, TLabel> Node { get; private set; }

        public IEnumerable<LayeredNode<TValue, TLabel>> Successors { get; private set; }
    }
}
