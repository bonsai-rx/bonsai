using System;
using System.Drawing;
using System.Windows.Forms;
using Bonsai.Design;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.Docking
{
    internal class WorkflowFloatWindow : FloatWindow
    {
        const int WM_CLOSE = 0x0010;
        CommandExecutor commandExecutor;

        protected internal WorkflowFloatWindow(DockPanel dockPanel, DockPane pane, IServiceProvider provider)
            : base(dockPanel, pane)
        {
            InitializeWindow(provider);
        }

        protected internal WorkflowFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds, IServiceProvider provider)
            : base(dockPanel, pane, bounds)
        {
            InitializeWindow(provider);
        }

        void InitializeWindow(IServiceProvider provider)
        {
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            FormBorderStyle = FormBorderStyle.Sizable;
            DoubleClickTitleBarToDock = false;
            ShowInTaskbar = true;
        }

        protected override void WndProc(ref Message m)
        {
            var closingPanes = m.Msg == WM_CLOSE && NestedPanes.Count > 0;
            if (closingPanes)
            {
                commandExecutor.BeginCompositeCommand();
            }
            base.WndProc(ref m);
            if (closingPanes)
            {
                commandExecutor.EndCompositeCommand();
            }
        }
    }
}
