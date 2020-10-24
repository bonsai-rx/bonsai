using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.Dag
{
    /// <summary>
    /// Represents a serializable descriptor of the nodes and edges in a directed graph.
    /// </summary>
    /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
    /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
    [Obsolete]
    public class DirectedGraphDescriptor<TNodeValue, TEdgeLabel>
    {
        /// <summary>
        /// Gets the collection of labels associated with each node in the directed graph.
        /// </summary>
        public Collection<TNodeValue> Nodes { get; } = new Collection<TNodeValue>();

        /// <summary>
        /// Gets a collection of descriptors corresponding to each edge in the directed graph.
        /// </summary>
        [XmlArrayItem("Edge")]
        public Collection<EdgeDescriptor<TEdgeLabel>> Edges { get; } = new Collection<EdgeDescriptor<TEdgeLabel>>();
    }
}
