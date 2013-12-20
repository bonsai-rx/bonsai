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
    public class WorkflowEditorLauncher : DialogLauncher
    {
        bool userClosing;
        ExpressionBuilderGraph workflow;
        WorkflowExpressionBuilder builder;
        WorkflowGraphView workflowGraphView;

        public WorkflowEditorLauncher(ExpressionBuilderGraph workflow, WorkflowExpressionBuilder builder)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            this.workflow = workflow;
            this.builder = builder;
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
                        workflowGraphView.CloseWorkflowEditorLauncher(this);
                    }
                    else UpdateEditorLayout();
                }
            };

            workflowGraphView = new WorkflowGraphView(provider);
            workflowGraphView.Launcher = this;
            workflowGraphView.Dock = DockStyle.Fill;
            workflowGraphView.Size = new Size(300, 200);
            workflowGraphView.VisualizerLayout = VisualizerLayout;
            workflowGraphView.Workflow = builder.Workflow;
            visualizerDialog.Padding = new Padding(10);
            visualizerDialog.AddControl(workflowGraphView);
        }
    }
}
