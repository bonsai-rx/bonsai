using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.Docking
{
    internal class WorkflowDockPaneCaptionFactory : DockPanelExtender.IDockPaneCaptionFactory
    {
        public DockPaneCaptionBase CreateDockPaneCaption(DockPane pane)
        {
            return new WorkflowDockPaneCaption(pane);
        }
    }
}
