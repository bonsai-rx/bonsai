using System;
using System.ComponentModel;
using System.Windows.Forms;
using Bonsai.Design;
using Bonsai.Editor.GraphView;
using WeifenLuo.WinFormsUI.Docking;

namespace Bonsai.Editor.Docking
{
    partial class WorkflowDockContent : DockContent
    {
        const string ReadOnlyPrefix = "🔒 ";
        readonly IWorkflowEditorService editorService;
        readonly WorkflowSelectionModel selectionModel;
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
            selectionModel = (WorkflowSelectionModel)provider.GetService(typeof(WorkflowSelectionModel));
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
        }

        public WorkflowGraphView WorkflowGraphView { get; }

        protected override string GetPersistString()
        {
            return DockPanelSerializer.SerializeContent(this);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            if (Parent != null && Pane is DockPane dockPane)
                dockPane.Tag ??= Tag;
            base.OnParentChanged(e);
        }

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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (!SelectNextContent(Pane?.Contents))
                if (!SelectNextContent(DockPanel.Contents))
                    selectionModel.UpdateSelection(null);
            base.OnFormClosed(e);
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
                    var forward = !shift;
                    if (!SelectNextContent(DockPanel.Contents, forward))
                        editorService.SelectNextControl(forward);
                }

                if (keyData == openNewTabToolStripMenuItem.ShortcutKeys &&
                    WorkflowGraphView.GraphView.SelectedNode is null)
                {
                    openNewTabToolStripMenuItem_Click(this, EventArgs.Empty);
                    return true;
                }

                if (keyData == openNewWindowToolStripMenuItem.ShortcutKeys &&
                    WorkflowGraphView.GraphView.SelectedNode is null)
                {
                    openNewWindowToolStripMenuItem_Click(this, EventArgs.Empty);
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private bool SelectNextContent(DockContentCollection contents, bool? forward = null)
        {
            if (contents is null)
                return false;

            int contentIndex = -1;
            WorkflowDockContent nextContent = null;
            for (int i = contents.Count - 1; i >= 0; i--)
            {
                if (contentIndex < 0 && contents[i] == this)
                {
                    contentIndex = i;
                    if (!forward.HasValue && nextContent is not null ||
                        forward.GetValueOrDefault())
                        break;
                    else
                        continue;
                }

                if (contents[i] is WorkflowDockContent dockContent && !dockContent.IsHidden)
                {
                    nextContent = dockContent;
                    if (contentIndex >= 0)
                        break;
                }
            }

            if (contentIndex >= 0 && nextContent is not null)
            {
                nextContent.Activate();
                return true;
            }
            return false;
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

        private void openNewTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorService.NavigateTo(WorkflowGraphView.WorkflowPath, NavigationPreference.NewTab);
        }

        private void openNewWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            editorService.NavigateTo(WorkflowGraphView.WorkflowPath, NavigationPreference.NewWindow);
        }
    }
}
