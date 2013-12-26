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
        public GraphEdge(object label, GraphNode successor)
            : this(null, label, successor)
        {
        }

        public GraphEdge(ITypeDescriptorContext context, object label, GraphNode successor)
        {
            Label = label;
            Node = successor;

            Text = string.Empty;
            Pen = Pens.Black;
            if (label != null)
            {
                var typeConverter = TypeDescriptor.GetConverter(label);
                Text = typeConverter.ConvertToString(context, label);
                if (typeConverter.CanConvertTo(context, typeof(Pen)))
                {
                    Pen = (Pen)typeConverter.ConvertTo(context, null, label, typeof(Pen));
                }
            }
        }

        public object Label { get; private set; }

        public GraphNode Node { get; private set; }

        public object Tag { get; set; }

        public string Text { get; private set; }

        public Pen Pen { get; private set; }
    }
}
