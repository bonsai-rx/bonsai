using System.Windows.Forms;

namespace Bonsai.Editor.GraphView
{
    class GraphNodeMouseEventArgs : MouseEventArgs
    {
        public GraphNodeMouseEventArgs(GraphNode node, MouseButtons button, int clicks, int x, int y, int delta)
            : base(button, clicks, x, y, delta)
        {
            Node = node;
        }

        public GraphNode Node { get; private set; }
    }
}
