using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a serializable descriptor of the nodes and edges in an expression builder graph.
    /// </summary>
    public class ExpressionBuilderGraphDescriptor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderGraphDescriptor"/> class.
        /// </summary>
        public ExpressionBuilderGraphDescriptor()
        {
            Nodes = new Collection<ExpressionBuilder>();
            Edges = new Collection<ExpressionBuilderArgumentDescriptor>();
        }

        internal ExpressionBuilderGraphDescriptor(
            IList<ExpressionBuilder> nodes,
            IList<ExpressionBuilderArgumentDescriptor> edges)
        {
            Nodes = new Collection<ExpressionBuilder>(nodes);
            Edges = new Collection<ExpressionBuilderArgumentDescriptor>(edges);
        }

        /// <summary>
        /// Gets the collection of labels associated with each node in the expression builder graph.
        /// </summary>
        public Collection<ExpressionBuilder> Nodes { get; }

        /// <summary>
        /// Gets a collection of descriptors corresponding to each edge in the expression builder graph.
        /// </summary>
        [XmlArrayItem("Edge")]
        public Collection<ExpressionBuilderArgumentDescriptor> Edges { get; }
    }
}
