using System.Collections.Generic;

namespace Bonsai.Editor.GraphModel
{
    interface IGraphView
    {
        IEnumerable<GraphNodeGrouping> Nodes { get; }

        IEnumerable<GraphNode> SelectedNodes { get; }
    }
}
