using System;
using System.Windows.Forms;
using Bonsai.Expressions;
using System.ComponentModel;
using Bonsai.Editor.GraphView;

namespace Bonsai.Design
{
    class WorkflowEditorLauncher : DialogLauncher
    {
        bool userClosing;
        readonly Func<WorkflowGraphView> parentSelector;
        readonly Func<WorkflowEditorControl> containerSelector;

        public WorkflowEditorLauncher(IWorkflowExpressionBuilder builder, Func<WorkflowGraphView> parentSelector, Func<WorkflowEditorControl> containerSelector)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.parentSelector = parentSelector ?? throw new ArgumentNullException(nameof(parentSelector));
            this.containerSelector = containerSelector ?? throw new ArgumentNullException(nameof(containerSelector));
        }

        internal IWorkflowExpressionBuilder Builder { get; }

        internal WorkflowGraphView ParentView
        {
            get { return parentSelector(); }
        }

        internal WorkflowEditorControl Container
        {
            get { return containerSelector(); }
        }

        internal IWin32Window Owner
        {
            get { return VisualizerDialog; }
        }

        public VisualizerLayout VisualizerLayout { get; set; }

        public WorkflowGraphView WorkflowGraphView { get; private set; }

        public void UpdateEditorLayout()
        {
            if (WorkflowGraphView != null)
            {
                WorkflowGraphView.UpdateVisualizerLayout();
                VisualizerLayout = WorkflowGraphView.VisualizerLayout;
                if (VisualizerDialog != null)
                {
                    Bounds = VisualizerDialog.DesktopBounds;
                }
            }
        }

        public void UpdateEditorText()
        {
            if (VisualizerDialog != null)
            {
                VisualizerDialog.Text = ExpressionBuilder.GetElementDisplayName(Builder);
                if (VisualizerDialog.TopLevel == false)
                {
                    Container.RefreshTab(Builder);
                }
            }
        }

        public override void Show(IWin32Window owner, IServiceProvider provider)
        {
            if (VisualizerDialog != null && VisualizerDialog.TopLevel == false)
            {
                Container.SelectTab(Builder);
            }
            else base.Show(owner, provider);
        }

        public override void Hide()
        {
            if (VisualizerDialog != null)
            {
                userClosing = false;
                if (VisualizerDialog.TopLevel == false)
                {
                    Container.CloseTab(Builder);
                }
                else base.Hide();
            }
        }

        void EditorClosing(object sender, CancelEventArgs e)
        {
            if (userClosing)
            {
                e.Cancel = true;
                ParentView.CloseWorkflowView(Builder);
                ParentView.UpdateSelection();
            }
            else
            {
                UpdateEditorLayout();
                WorkflowGraphView.HideEditorMapping();
            }
        }

        protected override TypeVisualizerDialog CreateVisualizerDialog(IServiceProvider provider)
        {
            return new NestedEditorDialog(provider);
        }

        protected override void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider)
        {
            var workflowEditor = Container;
            if (workflowEditor == null)
            {
                workflowEditor = new WorkflowEditorControl(provider, ParentView.ReadOnly);
                workflowEditor.SuspendLayout();
                workflowEditor.Dock = DockStyle.Fill;
                workflowEditor.Font = ParentView.Font;
                workflowEditor.Workflow = Builder.Workflow;
                WorkflowGraphView = workflowEditor.WorkflowGraphView;
                workflowEditor.ResumeLayout(false);
                visualizerDialog.AddControl(workflowEditor);
                visualizerDialog.Icon = Editor.Properties.Resources.Icon;
                visualizerDialog.ShowIcon = true;
                visualizerDialog.Activated += (sender, e) => workflowEditor.ActiveTab.UpdateSelection();
                visualizerDialog.FormClosing += (sender, e) =>
                {
                    if (e.CloseReason == CloseReason.UserClosing)
                    {
                        EditorClosing(sender, e);
                    }
                };
            }
            else
            {
                visualizerDialog.FormBorderStyle = FormBorderStyle.None;
                visualizerDialog.Dock = DockStyle.Fill;
                visualizerDialog.TopLevel = false;
                visualizerDialog.Visible = true;
                var tabState = workflowEditor.CreateTab(Builder, ParentView.ReadOnly, visualizerDialog);
                WorkflowGraphView = tabState.WorkflowGraphView;
                tabState.TabClosing += EditorClosing;
            }

            userClosing = true;
            visualizerDialog.BackColor = ParentView.ParentForm.BackColor;
            WorkflowGraphView.BackColorChanged += (sender, e) => visualizerDialog.BackColor = ParentView.ParentForm.BackColor;
            WorkflowGraphView.Launcher = this;
            WorkflowGraphView.VisualizerLayout = VisualizerLayout;
            WorkflowGraphView.SelectFirstGraphNode();
            WorkflowGraphView.Select();
            UpdateEditorText();
        }
    }
}
