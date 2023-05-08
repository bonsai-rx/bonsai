using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
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

        static void AssertIsSequenceEqual(
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> expected,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> actual)
        {
            expected = expected.ToArray();
            actual = actual.ToArray();
            if (!expected.SequenceEqual(actual))
            {
                string ToString(IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> sequence)
                {
                    return string.Join(",", sequence.Select(
                        node => ExpressionBuilder.GetElementDisplayName(node.Value)));
                }
                var expectedString = ToString(expected);
                var actualString = ToString(actual);
                Assert.Fail($"Sequence is not equal. Expected: {expectedString}. Actual: {actualString}");
            }
        }

        (WorkflowEditor editor, Action assertIsReversible) CreateMockEditor(ExpressionBuilderGraph workflow = null)
        {
            var executor = new CommandExecutor();
            var serviceProvider = new ServiceContainer();
            serviceProvider.AddService(typeof(CommandExecutor), executor);
            var graphView = new MockGraphView(workflow);
            var editor = new WorkflowEditor(serviceProvider, graphView);
            editor.UpdateLayout.Subscribe(graphView.UpdateGraphLayout);
            editor.UpdateSelection.Subscribe(graphView.UpdateSelection);
            editor.Workflow = graphView.Workflow;

            var nodeSequence = editor.Workflow.ToArray();
            return (editor, assertIsReversible: () =>
            {
                while (executor.CanUndo)
                {
                    executor.Undo();
                }

                AssertIsSequenceEqual(nodeSequence, editor.Workflow);
            });
        }

        [TestMethod]
        public void ReorderGraphNode_DanglingBranchWithPredecessors_KeepPredecessorEdges()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("ReorderDanglingBranchWithPredecessors.bonsai");
            var nodeSequence = workflowBuilder.Workflow.ToArray();
            var (editor, assertIsReversible) = CreateMockEditor(workflowBuilder.Workflow);

            var branchLead = editor.Workflow[2];
            var sourceNode = editor.Workflow[5];
            var nodes = new[] { editor.FindGraphNode(sourceNode.Value) };
            var target = editor.FindGraphNode(editor.Workflow[1].Value);
            editor.ReorderGraphNodes(nodes, target);
            Assert.AreEqual(expected: 1, branchLead.Successors.Count);

            Assert.AreSame(branchLead, editor.FindGraphNodeTag(branchLead.Value));
            Assert.AreEqual(expected: 1, branchLead.Successors.Count);
            Assert.AreEqual(expected: 1, editor.Workflow.IndexOf(branchLead));
            Assert.AreEqual(expected: 3, editor.Workflow.IndexOf(sourceNode));
            assertIsReversible();
        }

        [TestMethod]
        public void ReorderGraphNode_ComponentWithHigherIndexIntoLowerIndex_ReorderComponentNodes()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("ReorderComponentWithHigherIndexIntoLowerIndex.bonsai");
            var nodeSequence = workflowBuilder.Workflow.ToArray();
            var (editor, assertIsReversible) = CreateMockEditor(workflowBuilder.Workflow);

            // disconnect C from D
            var sourceNode = editor.Workflow[3];
            var targetNode = editor.Workflow[2];
            var nodes = new[] { editor.FindGraphNode(targetNode.Value) };
            var target = editor.FindGraphNode(sourceNode.Value);
            editor.DisconnectGraphNodes(nodes, target);

            // reorder D into C
            nodes = new[] { editor.FindGraphNode(sourceNode.Value) };
            target = editor.FindGraphNode(targetNode.Value);
            editor.ReorderGraphNodes(nodes, target);

            Assert.AreEqual(expected: editor.Workflow.Count - 1, editor.Workflow.IndexOf(targetNode));
            assertIsReversible();
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
