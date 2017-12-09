using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Design;
using Bonsai.Expressions;
using Bonsai.Dag;

namespace Bonsai.Design
{
    class WorkflowEditorLauncher : DialogLauncher
    {
        bool userClosing;
        IWorkflowExpressionBuilder builder;
        WorkflowEditorControl workflowEditor;
        WorkflowGraphView workflowGraphView;
        Func<WorkflowGraphView> parentSelector;

        public WorkflowEditorLauncher(IWorkflowExpressionBuilder builder, Func<WorkflowGraphView> parentSelector)
            : this(builder, parentSelector, null)
        {
        }

        public WorkflowEditorLauncher(IWorkflowExpressionBuilder builder, Func<WorkflowGraphView> parentSelector, WorkflowEditorControl container)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (parentSelector == null)
            {
                throw new ArgumentNullException("parentSelector");
            }

            this.builder = builder;
            this.workflowEditor = container;
            this.parentSelector = parentSelector;
        }

        internal WorkflowGraphView ParentView
        {
            get { return parentSelector(); }
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

        public override void Show(IWin32Window owner, IServiceProvider provider)
        {
            if (VisualizerDialog != null && VisualizerDialog.TopLevel == false)
            {
                workflowEditor.SelectTab(builder);
            }
            else base.Show(owner, provider);
        }

        public override void Hide()
        {
            userClosing = false;
            if (VisualizerDialog != null && VisualizerDialog.TopLevel == false)
            {
                workflowEditor.CloseTab(builder);
            }
            else base.Hide();
        }

        protected override void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider)
        {
            userClosing = true;
            visualizerDialog.Activated += delegate
            {
                workflowGraphView.UpdateSelection();
                visualizerDialog.Text = ExpressionBuilder.GetElementDisplayName(builder);
            };

            visualizerDialog.FormClosing += (sender, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    if (userClosing)
                    {
                        e.Cancel = true;
                        ParentView.CloseWorkflowView(builder);
                        ParentView.UpdateSelection();
                    }
                    else UpdateEditorLayout();
                }
            };

            if (workflowEditor == null)
            {
                workflowEditor = new WorkflowEditorControl(provider, ParentView.ReadOnly);
                workflowEditor.SuspendLayout();
                workflowEditor.Dock = DockStyle.Fill;
                workflowEditor.Font = ParentView.Font;
                workflowEditor.Workflow = builder.Workflow;
                workflowEditor.VisualizerLayout = VisualizerLayout;
                workflowGraphView = workflowEditor.WorkflowGraphView;
                workflowEditor.ResumeLayout(false);
                visualizerDialog.AddControl(workflowEditor);
                visualizerDialog.Icon = Bonsai.Editor.Properties.Resources.Icon;
                visualizerDialog.ShowIcon = true;
                visualizerDialog.HandleDestroyed += (sender, e) => workflowEditor = null;
            }
            else
            {
                visualizerDialog.FormBorderStyle = FormBorderStyle.None;
                visualizerDialog.Dock = DockStyle.Fill;
                visualizerDialog.TopLevel = false;
                visualizerDialog.Visible = true;
                workflowGraphView = workflowEditor.ShowTab(builder, visualizerDialog);
            }

            workflowGraphView.Launcher = this;
        }
    }
}
