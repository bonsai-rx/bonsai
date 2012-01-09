using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Dag
{
    public struct Edge<TValue, TLabel>
    {
        public Edge(TLabel label, Node<TValue, TLabel> successor)
            : this()
        {
            Label = label;
            Node = successor;
        }

        public TLabel Label { get; private set; }

        public Node<TValue, TLabel> Node { get; private set; }
    }
}
