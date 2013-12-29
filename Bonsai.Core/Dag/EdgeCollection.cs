using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Bonsai.Dag
{
    /// <summary>
    /// Represents a collection of outgoing labeled edges in a directed graph.
    /// </summary>
    /// <typeparam name="TNodeValue">The type of the labels associated with graph nodes.</typeparam>
    /// <typeparam name="TEdgeLabel">The type of the labels associated with graph edges.</typeparam>
    public class EdgeCollection<TNodeValue, TEdgeLabel> : Collection<Edge<TNodeValue, TEdgeLabel>>
    {
    }
}
