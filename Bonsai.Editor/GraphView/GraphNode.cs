using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace Bonsai.Design
{
    class GraphNode
    {
        readonly TypeConverter typeConverter;

        public GraphNode(object value, int layer, IEnumerable<GraphEdge> successors)
        {
            Value = value;
            Layer = layer;
            Successors = successors;

            Pen = Pens.Black;
            Brush = Brushes.White;
            if (value != null)
            {
                typeConverter = TypeDescriptor.GetConverter(value);
                if (typeConverter.CanConvertTo(typeof(Brush)))
                {
                    Brush = (Brush)typeConverter.ConvertTo(value, typeof(Brush));
                }

                if (typeConverter.CanConvertTo(typeof(Image)))
                {
                    Image = (Image)typeConverter.ConvertTo(value, typeof(Image));
                }

                if (typeConverter.CanConvertTo(typeof(WorkflowIcon)))
                {
                    Icon = (WorkflowIcon)typeConverter.ConvertTo(value, typeof(WorkflowIcon));
                }

                if (typeConverter.CanConvertTo(typeof(Pen)))
                {
                    Pen = (Pen)typeConverter.ConvertTo(value, typeof(Pen));
                }
            }

            if (Pen != null)
            {
                InitializeDummySuccessors();
            }
        }

        void InitializeDummySuccessors()
        {
            foreach (var successor in Successors)
            {
                if (successor.Node.Value == null)
                {
                    successor.Node.Pen = Pen;
                    successor.Node.InitializeDummySuccessors();
                }
            }
        }

        public bool Highlight { get; set; }

        public int Layer { get; private set; }

        public int LayerIndex { get; internal set; }

        public object Value { get; private set; }

        public IEnumerable<GraphEdge> Successors { get; private set; }

        public object Tag { get; set; }

        public Brush Brush { get; private set; }

        public Image Image { get; private set; }

        public WorkflowIcon Icon { get; private set; }

        public Pen Pen { get; private set; }

        public string Text
        {
            get { return typeConverter != null ? typeConverter.ConvertToString(Value) : string.Empty; }
        }

        /// <summary>
        /// Returns a string that represents the value of this <see cref="GraphNode"/> instance.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="GraphNode"/> object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{{{0}}}", Text);
        }
    }
}
