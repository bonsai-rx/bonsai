using System.Windows.Forms;
using Bonsai.Editor.GraphModel;

namespace Bonsai.Editor.GraphView
{
    internal class WorkflowPathMouseEventArgs : MouseEventArgs
    {
        public WorkflowPathMouseEventArgs(WorkflowEditorPath path, MouseButtons button, int clicks, int x, int y, int delta)
            : base(button, clicks, x, y, delta)
        {
            Path = path;
        }

        public WorkflowEditorPath Path { get; }
    }
}
