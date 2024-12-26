using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.GraphView
{
    internal class WorkflowFloatWindow : FloatWindow
    {
        protected internal WorkflowFloatWindow(DockPanel dockPanel, DockPane pane)
            : base(dockPanel, pane)
        {
            InitializeWindow();
        }

        protected internal WorkflowFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
            : base(dockPanel, pane, bounds)
        {
            InitializeWindow();
        }

        void InitializeWindow()
        {
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleClickTitleBarToDock = false;
            ShowInTaskbar = true;
        }
    }
}
