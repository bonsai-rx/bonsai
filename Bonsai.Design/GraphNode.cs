using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace Bonsai.Design
{
    public class GraphNode
    {
        public GraphNode(object value, int layer, IEnumerable<GraphEdge> successors)
        {
            Value = value;
            Layer = layer;
            Successors = successors;

            Text = string.Empty;
            Brush = Brushes.White;
            if (value != null)
            {
                var typeConverter = TypeDescriptor.GetConverter(value);
                Text = typeConverter.ConvertToString(value);
                if (typeConverter.CanConvertTo(typeof(Brush)))
                {
                    Brush = (Brush)typeConverter.ConvertTo(value, typeof(Brush));
                }
            }
        }

        public int Layer { get; private set; }

        public int LayerIndex { get; internal set; }

        public object Value { get; private set; }

        public IEnumerable<GraphEdge> Successors { get; private set; }

        public object Tag { get; set; }

        public string Text { get; private set; }

        public Brush Brush { get; private set; }
    }
}
