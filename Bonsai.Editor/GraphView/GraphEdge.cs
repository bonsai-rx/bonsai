using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace Bonsai.Design
{
    class GraphEdge
    {
        public GraphEdge(ITypeDescriptorContext context, object label, GraphNode successor)
        {
            Label = label;
            Node = successor;

            Text = string.Empty;
            if (label != null)
            {
                var typeConverter = TypeDescriptor.GetConverter(label);
                Text = typeConverter.ConvertToString(context, label);
            }
        }

        internal GraphEdge(GraphEdge edge, GraphNode successor)
        {
            Label = edge.Label;
            Node = successor;
            Text = edge.Text;
        }

        public object Label { get; private set; }

        public GraphNode Node { get; set; }

        public object Tag { get; set; }

        public string Text { get; private set; }
    }
}
