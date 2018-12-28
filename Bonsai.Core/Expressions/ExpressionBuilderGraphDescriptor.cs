using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a serializable descriptor of the nodes and edges in an expression builder graph.
    /// </summary>
    public class ExpressionBuilderGraphDescriptor
    {
        readonly Collection<ExpressionBuilder> nodes = new Collection<ExpressionBuilder>();
        readonly Collection<ExpressionBuilderArgumentDescriptor> edges = new Collection<ExpressionBuilderArgumentDescriptor>();

        /// <summary>
        /// Gets the collection of labels associated with each node in the expression builder graph.
        /// </summary>
        public Collection<ExpressionBuilder> Nodes
        {
            get { return nodes; }
        }

        /// <summary>
        /// Gets a collection of descriptors corresponding to each edge in the expression builder graph.
        /// </summary>
        [XmlArrayItem("Edge")]
        public Collection<ExpressionBuilderArgumentDescriptor> Edges
        {
            get { return edges; }
        }
    }
}
