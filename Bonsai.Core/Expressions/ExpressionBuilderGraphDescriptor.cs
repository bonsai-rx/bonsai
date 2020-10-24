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
        /// Gets the collection of labels associated with each node in the expression builder graph.
        /// </summary>
        public Collection<ExpressionBuilder> Nodes { get; } = new Collection<ExpressionBuilder>();

        /// <summary>
        /// Gets a collection of descriptors corresponding to each edge in the expression builder graph.
        /// </summary>
        [XmlArrayItem("Edge")]
        public Collection<ExpressionBuilderArgumentDescriptor> Edges { get; } = new Collection<ExpressionBuilderArgumentDescriptor>();
    }
}
