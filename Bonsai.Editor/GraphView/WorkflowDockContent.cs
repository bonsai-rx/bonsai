using System;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.GraphView
{
    partial class WorkflowDockContent : DockContent
    {
        const string ReadOnlySuffix = " [Read-only]";

        public WorkflowDockContent(WorkflowGraphView graphView)
        {
            InitializeComponent();
            WorkflowGraphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
        }

        public WorkflowGraphView WorkflowGraphView { get; }

        protected override void OnActivated(EventArgs e)
        {
            WorkflowGraphView.UpdateSelection();
            base.OnActivated(e);
        }

        protected override void OnEnter(EventArgs e)
        {
            WorkflowGraphView.UpdateSelection();
            base.OnEnter(e);
        }

        public void UpdateText()
        {
            Text = WorkflowGraphView.DisplayName + (WorkflowGraphView.IsReadOnly ? ReadOnlySuffix : string.Empty);
        }
    }
}
