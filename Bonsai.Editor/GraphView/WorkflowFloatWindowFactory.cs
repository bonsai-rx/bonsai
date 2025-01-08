using System;
using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.GraphView
{
    internal class WorkflowFloatWindowFactory : DockPanelExtender.IFloatWindowFactory
    {
        readonly IServiceProvider serviceProvider;

        public WorkflowFloatWindowFactory(IServiceProvider provider)
        {
            serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane)
        {
            return new WorkflowFloatWindow(dockPanel, pane, serviceProvider);
        }

        public FloatWindow CreateFloatWindow(DockPanel dockPanel, DockPane pane, Rectangle bounds)
        {
            return new WorkflowFloatWindow(dockPanel, pane, bounds, serviceProvider);
        }
    }
}
