using System.Collections.Generic;
using System.Linq;
using Bonsai.Editor.GraphModel;
using Bonsai.Expressions;

namespace Bonsai.Editor.Tests
{
    static class EditorHelper
    {
        static ExpressionBuilder CreateBuilder(string name)
        {
            var nestedGraph = new ExpressionBuilderGraph();
            nestedGraph.Add(new WorkflowInputBuilder());
            return new GroupWorkflowBuilder(nestedGraph) { Name = name };
        }

        internal static ExpressionBuilderGraph CreateEditorGraph(params string[] values)
        {
            var graph = new ExpressionBuilderGraph();
            for (int i = 0; i < values.Length; i++)
            {
                var builder = CreateBuilder(values[i]);
                graph.Add(builder);
            }
            return graph.ToInspectableGraph();
        }

        internal static TBuilder FindExpressionBuilder<TBuilder>(this ExpressionBuilderGraph workflow) where TBuilder : class
        {
            return (from node in workflow
                    let builder = node.Value as TBuilder
                    where builder != null
                    select builder).FirstOrDefault();
        }

        internal static GraphNode FindNode(this WorkflowEditor editor, string name)
        {
            var node = editor.Workflow.First(n => ExpressionBuilder.GetElementDisplayName(n.Value) == name);
            return editor.FindGraphNode(node.Value);
        }

        internal static GraphNode FindNode(this WorkflowEditor editor, ExpressionBuilder builder)
        {
            var node = editor.Workflow.First(n => ExpressionBuilder.Unwrap(n.Value) == builder);
            return editor.FindGraphNode(node.Value);
        }

        internal static void ConnectNodes(this WorkflowEditor editor, string source, string target)
        {
            var sourceNodes = new[] { editor.FindNode(source) };
            var targetNode = editor.FindNode(target);
            editor.ConnectGraphNodes(sourceNodes, targetNode);
        }

        internal static void CreateNode(
            this WorkflowEditor editor,
            string name,
            GraphNode selectedNode = null,
            CreateGraphNodeType nodeType = CreateGraphNodeType.Successor,
            bool branch = false)
        {
            var builder = CreateBuilder(name);
            editor.CreateGraphNode(builder, selectedNode, nodeType, branch);
        }

        internal static IEnumerable<string> GetGraphValues(this WorkflowEditor editor)
        {
            return editor.Workflow.Select(n => ExpressionBuilder.GetElementDisplayName(n.Value));
        }
    }
}
