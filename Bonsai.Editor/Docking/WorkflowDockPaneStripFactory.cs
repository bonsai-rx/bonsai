using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.Docking
{
    internal class WorkflowDockPaneStripFactory : DockPanelExtender.IDockPaneStripFactory
    {
        public DockPaneStripBase CreateDockPaneStrip(DockPane pane)
        {
            return new WorkflowDockPaneStrip(pane);
        }
    }
}
