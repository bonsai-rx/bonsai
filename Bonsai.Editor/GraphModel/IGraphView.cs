using System.Collections.Generic;

namespace Bonsai.Editor.GraphModel
{
    interface IGraphView
    {
        IReadOnlyList<GraphNodeGrouping> Nodes { get; }

        IEnumerable<GraphNode> SelectedNodes { get; }

        GraphNode CursorNode { get; }
    }
}
