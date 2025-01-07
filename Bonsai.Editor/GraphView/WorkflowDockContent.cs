using System;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.GraphView
{
    partial class WorkflowDockContent : DockContent
    {
        const string ReadOnlySuffix = " [Read-only]";
        readonly IWorkflowEditorService editorService;

        public WorkflowDockContent(WorkflowGraphView graphView, IServiceProvider provider)
        {
            InitializeComponent();
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (DockPanel != null)
            {
                var keys = keyData;
                var shift = keys.HasFlag(Keys.Shift);
                if (shift) keys &= ~Keys.Shift;
                var control = keys.HasFlag(Keys.Control);
                if (control) keys &= ~Keys.Control;

                if (control && keys == Keys.Tab)
                {
                    var offset = shift ? -1 : 1;
                    for (int i = 0; i < DockPanel.Contents.Count; i++)
                    {
                        if (DockPanel.Contents[i] == this)
                        {
                            var nextIndex = i + offset;
                            if (nextIndex < 0 || nextIndex >= DockPanel.Contents.Count)
                            {
                                editorService.SelectNextControl(nextIndex >= 0);
                                return true;
                            }
                            else if (DockPanel.Contents[nextIndex] is DockContent nextContent)
                            {
                                nextContent.Activate();
                                return true;
                            }
                        }
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        public void UpdateText()
        {
            Text = WorkflowGraphView.DisplayName + (WorkflowGraphView.IsReadOnly ? ReadOnlySuffix : string.Empty);
        }
    }
}
