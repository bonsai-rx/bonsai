using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a serializable descriptor of an edge connecting two nodes in an expression builder graph.
    /// </summary>
    public class ExpressionBuilderArgumentDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderArgumentDescriptor"/> class.
        /// </summary>
        public ExpressionBuilderArgumentDescriptor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderArgumentDescriptor"/> class
        /// with the specified indices for source and target nodes and a label value.
        /// </summary>
        /// <param name="from">The zero-based index of the node that is the source of the edge.</param>
        /// <param name="to">The zero-based index of the node that is the target of the edge.</param>
        /// <param name="label">The value of the edge label.</param>
        public ExpressionBuilderArgumentDescriptor(int from, int to, string label)
        {
            From = from;
            To = to;
            Label = label;
        }

        /// <summary>
        /// Gets or sets the zero-based index of the node that is the source of the edge.
        /// </summary>
        [XmlAttribute]
        public int From { get; set; }

        /// <summary>
        /// Gets or sets the zero-based index of the node that is the target of the edge.
        /// </summary>
        [XmlAttribute]
        public int To { get; set; }

        /// <summary>
        /// Gets or sets the value of the edge label.
        /// </summary>
        [XmlAttribute]
        public string Label { get; set; }
    }
}
