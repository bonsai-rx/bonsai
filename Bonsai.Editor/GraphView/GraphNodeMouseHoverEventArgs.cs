using System;

namespace Bonsai.Editor.GraphView
{
    class GraphNodeMouseHoverEventArgs : EventArgs
    {
        public GraphNodeMouseHoverEventArgs(GraphNode node)
        {
            Node = node;
        }

        public GraphNode Node { get; private set; }
    }
}
