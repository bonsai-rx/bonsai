using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public class GraphNodeMouseClickEventArgs : MouseEventArgs
    {
        public GraphNodeMouseClickEventArgs(GraphNode node, MouseButtons button, int clicks, int x, int y, int delta)
            : base(button, clicks, x, y, delta)
        {
            Node = node;
        }

        public GraphNode Node { get; private set; }
    }
}
