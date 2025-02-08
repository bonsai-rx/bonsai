using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.GraphView
{
    partial class WorkflowDockContent : DockContent
    {
        const string ReadOnlyPrefix = "🔒 ";
        readonly IWorkflowEditorService editorService;
        readonly CommandExecutor commandExecutor;

        public WorkflowDockContent(WorkflowGraphView graphView, IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            InitializeComponent();
            WorkflowGraphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
            editorService = (IWorkflowEditorService)provider.GetService(typeof(IWorkflowEditorService));
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
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
                var control = keys.HasFlag(Keys.Control);
                if (control) keys &= ~Keys.Control;
                if (control && keys == Keys.F4)
                {
                    Close();
                    return true;
                }

                var shift = keys.HasFlag(Keys.Shift);
                if (shift) keys &= ~Keys.Shift;
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
            Text = (WorkflowGraphView.IsReadOnly ? ReadOnlyPrefix : string.Empty) + WorkflowGraphView.DisplayName;
        }

        void CloseOther(DockPane pane)
        {
            var paneContents = new IDockContent[pane.Contents.Count];
            pane.Contents.CopyTo(paneContents, 0);
            for (int i = 0; i < paneContents.Length; i++)
            {
                if (paneContents[i] is WorkflowDockContent workflowContent && workflowContent != this)
                    workflowContent.Close();
            }
        }

        private void tabContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            closeOtherToolStripMenuItem.Enabled = DockPanel.Contents.Count > 1;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void closeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Pane is DockPane pane && pane.Contents.Count > 0)
            {
                commandExecutor.BeginCompositeCommand();
                Close();
                CloseOther(pane);
                commandExecutor.EndCompositeCommand();
            }
        }

        private void closeOtherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Pane is DockPane pane && pane.Contents.Count > 1)
            {
                commandExecutor.BeginCompositeCommand();
                CloseOther(pane);
                commandExecutor.EndCompositeCommand();
            }
        }
    }
}
