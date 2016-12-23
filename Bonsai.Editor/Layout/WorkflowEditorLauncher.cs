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
                if (!string.IsNullOrWhiteSpace(builder.Name))
                {
                    visualizerDialog.Text = builder.Name;
                }
                else visualizerDialog.Text = "Workflow Editor";
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

            workflowGraphView = new WorkflowGraphView(provider);
            workflowGraphView.SuspendLayout();
            workflowGraphView.Launcher = this;
            workflowGraphView.Font = ParentView.Font;
            workflowGraphView.Dock = DockStyle.Fill;
            workflowGraphView.AutoScaleDimensions = new SizeF(6F, 13F);
            workflowGraphView.Size = new Size(300, 200);
            workflowGraphView.Workflow = builder.Workflow;
            workflowGraphView.VisualizerLayout = VisualizerLayout;
            workflowGraphView.ResumeLayout(false);
            visualizerDialog.Padding = new Padding(10);
            visualizerDialog.AddControl(workflowGraphView);
        }
    }
}
