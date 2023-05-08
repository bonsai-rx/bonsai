using System;
using System.ComponentModel.Design;
using System.IO;
using System.Xml;
using Bonsai.Dag;
using Bonsai.Design;
using Bonsai.Editor.GraphModel;
using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Editor.Tests
{
    [TestClass]
    public class WorkflowEditorTests
    {
        static Stream LoadEmbeddedResource(string name)
        {
            var qualifierType = typeof(WorkflowEditorTests);
            var embeddedWorkflowStream = qualifierType.Namespace + "." + name;
            return qualifierType.Assembly.GetManifestResourceStream(embeddedWorkflowStream);
        }

        static WorkflowBuilder LoadEmbeddedWorkflow(string name)
        {
            using var workflowStream = LoadEmbeddedResource(name);
            using var reader = XmlReader.Create(workflowStream);
            reader.MoveToContent();
            return (WorkflowBuilder)WorkflowBuilder.Serializer.Deserialize(reader);
        }

        WorkflowEditor CreateMockEditor(ExpressionBuilderGraph workflow = null)
        {
            var executor = new CommandExecutor();
            var serviceProvider = new ServiceContainer();
            serviceProvider.AddService(typeof(CommandExecutor), executor);
            var graphView = new MockGraphView(workflow);
            var editor = new WorkflowEditor(serviceProvider, graphView);
            editor.UpdateLayout.Subscribe(graphView.UpdateGraphLayout);
            editor.UpdateSelection.Subscribe(graphView.UpdateSelection);
            editor.Workflow = graphView.Workflow;
            return editor;
        }

        [TestMethod]
        public void ReorderGraphNode_ReorderDanglingBranchWithPredecessors_KeepPredecessorEdges()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("ReorderDanglingBranchWithPredecessors.bonsai");
            var editor = CreateMockEditor(workflowBuilder.Workflow);
            var source = editor.Workflow[2];
            Assert.AreEqual(expected: 1, source.Successors.Count);

            var nodes = new[] { editor.FindGraphNode(editor.Workflow[4].Value) };
            var target = editor.FindGraphNode(editor.Workflow[1].Value);
            editor.ReorderGraphNodes(nodes, target);

            source = editor.FindGraphNodeTag(source.Value);
            Assert.AreEqual(expected: 1, source.Successors.Count);
        }
    }

    static class WorkflowEditorHelper
    {
        public static Node<ExpressionBuilder, ExpressionBuilderArgument> FindGraphNodeTag(
            this WorkflowEditor editor,
            ExpressionBuilder value)
        {
            return (Node<ExpressionBuilder, ExpressionBuilderArgument>)editor.FindGraphNode(value).Tag;
        }
    }
}
