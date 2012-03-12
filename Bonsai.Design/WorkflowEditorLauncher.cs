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
        WorkflowViewModel workflowModel;
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

        public void UpdateEditorLayout()
        {
            if (workflowModel != null)
            {
                workflowModel.UpdateVisualizerLayout();
                VisualizerLayout = workflowModel.VisualizerLayout;
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

            var graphView = new GraphView { AllowDrop = true, Dock = DockStyle.Fill, Size = new Size(300, 200) };
            visualizerDialog.Padding = new Padding(10);
            visualizerDialog.AddControl(graphView);

            workflowModel = new WorkflowViewModel(graphView, provider);
            workflowModel.VisualizerLayout = VisualizerLayout;
            workflowModel.Workflow = workflowExpressionBuilder.Workflow;
            if (!workflowModel.Workflow.Any(n => n.Value is WorkflowInputBuilder) && workflow.Predecessors(builderNode).Any())
            {
                workflowModel.CreateGraphNode(typeof(WorkflowInputBuilder).AssemblyQualifiedName, WorkflowElementType.Combinator, null, CreateGraphNodeType.Successor, false);
            }
        }
    }
}
