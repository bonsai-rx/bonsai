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
        ExpressionBuilderGraph workflow;
        WorkflowGraphView workflowGraphView;
        Node<ExpressionBuilder, ExpressionBuilderParameter> builderNode;

        public WorkflowEditorLauncher(ExpressionBuilderGraph workflow, Node<ExpressionBuilder, ExpressionBuilderParameter> builderNode)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException("workflow");
            }

            if (builderNode == null)
            {
                throw new ArgumentNullException("builderNode");
            }

            this.workflow = workflow;
            this.builderNode = builderNode;
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

        protected override void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider)
        {
            var workflowExpressionBuilder = builderNode.Value as WorkflowExpressionBuilder;
            visualizerDialog.Activated += delegate
            {
                if (workflowExpressionBuilder != null && !string.IsNullOrWhiteSpace(workflowExpressionBuilder.Name))
                {
                    visualizerDialog.Text = workflowExpressionBuilder.Name;
                }
                else visualizerDialog.Text = "Workflow Editor";
            };

            visualizerDialog.FormClosing += (sender, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    UpdateEditorLayout();
                }
            };

            workflowGraphView = new WorkflowGraphView(provider);
            workflowGraphView.Dock = DockStyle.Fill;
            workflowGraphView.Size = new Size(300, 200);
            workflowGraphView.VisualizerLayout = VisualizerLayout;
            workflowGraphView.Workflow = workflowExpressionBuilder.Workflow;
            visualizerDialog.Padding = new Padding(10);
            visualizerDialog.AddControl(workflowGraphView);
        }
    }
}
