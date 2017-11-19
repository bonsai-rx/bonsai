using Bonsai.Design;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    interface IWorkflowEditorView
    {
        GraphView GraphView { get; }

        VisualizerLayout VisualizerLayout { get; set; }

        ExpressionBuilderGraph Workflow { get; set; }

        GraphNode FindGraphNode(object value);

        void LaunchWorkflowView(GraphNode node);

        WorkflowEditorLauncher GetWorkflowEditorLauncher(GraphNode node);

        void UpdateVisualizerLayout();
    }
}
