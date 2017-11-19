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
        WorkflowExpressionBuilder builder;
        WorkflowEditorControl workflowEditor;
        WorkflowGraphView workflowGraphView;
        Func<WorkflowGraphView> parentSelector;

        public WorkflowEditorLauncher(WorkflowExpressionBuilder builder, Func<WorkflowGraphView> parentSelector)
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

        public override void Hide()
        {
            userClosing = false;
            base.Hide();
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

            workflowEditor = new WorkflowEditorControl(provider);
            workflowEditor.SuspendLayout();
            workflowEditor.Font = ParentView.Font;
            workflowEditor.Dock = DockStyle.Fill;
            workflowEditor.Size = new Size(300, 200);
            workflowEditor.AutoScaleDimensions = new SizeF(6F, 13F);
            workflowEditor.Workflow = builder.Workflow;
            workflowEditor.VisualizerLayout = VisualizerLayout;

            workflowGraphView = workflowEditor.WorkflowGraphView;
            workflowGraphView.Launcher = this;
            workflowEditor.ResumeLayout(false);
            visualizerDialog.AddControl(workflowEditor);
            visualizerDialog.Icon = Bonsai.Editor.Properties.Resources.Icon;
            visualizerDialog.ShowIcon = true;
        }
    }
}
