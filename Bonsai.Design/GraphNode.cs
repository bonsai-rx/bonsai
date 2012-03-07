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
        readonly TypeConverter typeConverter;

        public GraphNode(object value, int layer, IEnumerable<GraphEdge> successors)
        {
            Value = value;
            Layer = layer;
            Successors = successors;

            Brush = Brushes.White;
            if (value != null)
            {
                typeConverter = TypeDescriptor.GetConverter(value);
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

        public Brush Brush { get; private set; }

        public string Text
        {
            get { return typeConverter != null ? typeConverter.ConvertToString(Value) : string.Empty; }
        }
    }
}
