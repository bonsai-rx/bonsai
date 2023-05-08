using System.Collections.Generic;
using System.Linq;
using Bonsai.Editor.GraphModel;
using Bonsai.Expressions;

namespace Bonsai.Editor.Tests
{
    class MockGraphView : IGraphView
    {
        ExpressionBuilderGraph workflow;
        readonly HashSet<GraphNode> selectedNodes = new HashSet<GraphNode>();

        public MockGraphView(ExpressionBuilderGraph workflow = null)
        {
            Workflow = workflow ?? new ExpressionBuilderGraph();
        }

        public ExpressionBuilderGraph Workflow
        {
            get { return workflow; }
            set
            {
                workflow = value;
                UpdateGraphLayout();
            }
        }

        public IReadOnlyList<GraphNodeGrouping> Nodes { get; set; }

        public IEnumerable<GraphNode> SelectedNodes
        {
            get { return selectedNodes; }
            set
            {
                if (selectedNodes != value)
                {
                    selectedNodes.Clear();
                    if (value != null)
                    {
                        foreach (var node in value)
                        {
                            selectedNodes.Add(node);
                            CursorNode = node;
                        }
                    }
                }
            }
        }

        public GraphNode CursorNode { get; set; }

        public void UpdateSelection(IEnumerable<ExpressionBuilder> selection)
        {
            SelectedNodes = Nodes
                .LayeredNodes()
                .Where(node =>
                {
                    var nodeBuilder = WorkflowEditor.GetGraphNodeBuilder(node);
                    return selection.Any(builder => ExpressionBuilder.Unwrap(builder) == nodeBuilder);
                });
        }

        public void UpdateGraphLayout()
        {
            Nodes = Workflow.ConnectedComponentLayering();
        }

        public void UpdateGraphLayout(bool _)
        {
            UpdateGraphLayout();
        }
    }
}
