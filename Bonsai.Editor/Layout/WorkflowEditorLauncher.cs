using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Design;
using Bonsai.Expressions;
using Bonsai.Dag;
using System.ComponentModel;
using Bonsai.Editor.GraphView;

namespace Bonsai.Design
{
    class WorkflowEditorLauncher : DialogLauncher
    {
        bool userClosing;
        IWorkflowExpressionBuilder builder;
        WorkflowGraphView workflowGraphView;
        Func<WorkflowGraphView> parentSelector;
        Func<WorkflowEditorControl> containerSelector;

        public WorkflowEditorLauncher(IWorkflowExpressionBuilder builder, Func<WorkflowGraphView> parentSelector, Func<WorkflowEditorControl> containerSelector)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (parentSelector == null)
            {
                throw new ArgumentNullException("parentSelector");
            }

            if (containerSelector == null)
            {
                throw new ArgumentNullException("containerSelector");
            }

            this.builder = builder;
            this.parentSelector = parentSelector;
            this.containerSelector = containerSelector;
        }

        internal IWorkflowExpressionBuilder Builder
        {
            get { return builder; }
        }

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

        public WorkflowGraphView WorkflowGraphView
        {
            get { return workflowGraphView; }
        }

        public void UpdateEditorLayout()
        {
            if (workflowGraphView != null)
            {
                workflowGraphView.UpdateVisualizerLayout();
                VisualizerLayout = workflowGraphView.VisualizerLayout;
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
                VisualizerDialog.Text = ExpressionBuilder.GetElementDisplayName(builder);
                if (VisualizerDialog.TopLevel == false)
                {
                    Container.RefreshTab(builder);
                }
            }
        }

        public override void Show(IWin32Window owner, IServiceProvider provider)
        {
            if (VisualizerDialog != null && VisualizerDialog.TopLevel == false)
            {
                Container.SelectTab(builder);
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
                    Container.CloseTab(builder);
                }
                else base.Hide();
            }
        }

        void EditorClosing(object sender, CancelEventArgs e)
        {
            if (userClosing)
            {
                e.Cancel = true;
                ParentView.CloseWorkflowView(builder);
                ParentView.UpdateSelection();
            }
            else
            {
                UpdateEditorLayout();
                workflowGraphView.HideEditorMapping();
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
                workflowEditor.Workflow = builder.Workflow;
                workflowGraphView = workflowEditor.WorkflowGraphView;
                workflowEditor.ResumeLayout(false);
                visualizerDialog.AddControl(workflowEditor);
                visualizerDialog.Icon = Bonsai.Editor.Properties.Resources.Icon;
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
                var tabState = workflowEditor.CreateTab(builder, ParentView.ReadOnly, visualizerDialog);
                workflowGraphView = tabState.WorkflowGraphView;
                tabState.TabClosing += EditorClosing;
            }

            userClosing = true;
            visualizerDialog.BackColor = ParentView.ParentForm.BackColor;
            workflowGraphView.BackColorChanged += (sender, e) => visualizerDialog.BackColor = ParentView.ParentForm.BackColor;
            workflowGraphView.Launcher = this;
            workflowGraphView.VisualizerLayout = VisualizerLayout;
            workflowGraphView.SelectFirstGraphNode();
            workflowGraphView.Select();
            UpdateEditorText();
        }
    }
}
