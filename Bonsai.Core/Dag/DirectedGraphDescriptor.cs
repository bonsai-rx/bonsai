using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        readonly Collection<TNodeValue> nodes = new Collection<TNodeValue>();
        readonly Collection<EdgeDescriptor<TEdgeLabel>> edges = new Collection<EdgeDescriptor<TEdgeLabel>>();

        /// <summary>
        /// Gets the collection of labels associated with each node in the directed graph.
        /// </summary>
        public Collection<TNodeValue> Nodes
        {
            get { return nodes; }
        }

        /// <summary>
        /// Gets a collection of descriptors corresponding to each edge in the directed graph.
        /// </summary>
        [XmlArrayItem("Edge")]
        public Collection<EdgeDescriptor<TEdgeLabel>> Edges
        {
            get { return edges; }
        }
    }
}
