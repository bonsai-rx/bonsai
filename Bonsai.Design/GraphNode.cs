using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Design
{
    public class GraphNode
    {
        public GraphNode(object value, int layer, IEnumerable<GraphNode> successors)
        {
            Value = value;
            Layer = layer;
            Successors = successors;
            Text = value != null ? TypeDescriptor.GetConverter(value).ConvertToString(value) : string.Empty;
        }

        public int Layer { get; private set; }

        public int LayerIndex { get; internal set; }

        public object Value { get; private set; }

        public IEnumerable<GraphNode> Successors { get; private set; }

        public object Tag { get; set; }

        public string Text { get; private set; }
    }
}
