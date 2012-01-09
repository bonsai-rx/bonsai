using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Bonsai.Dag
{
    public class Node<TValue, TLabel>
    {
        readonly EdgeCollection<TValue, TLabel> successors = new EdgeCollection<TValue, TLabel>();

        public Node(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; private set; }

        public EdgeCollection<TValue, TLabel> Successors
        {
            get { return successors; }
        }
    }
}
