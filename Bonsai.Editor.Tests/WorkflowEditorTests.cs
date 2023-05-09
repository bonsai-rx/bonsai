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

        [DataTestMethod]
        [DataRow(CreateGraphNodeType.Successor, false)]
        [DataRow(CreateGraphNodeType.Successor, true)]
        [DataRow(CreateGraphNodeType.Predecessor, false)]
        [DataRow(CreateGraphNodeType.Predecessor, true)]
        public void CreateGraphNode_EmptyWorkflow_SingleNode(object nodeType, bool branch)
        {
            var (editor, assertIsReversible) = CreateMockEditor();
            Assert.AreEqual(expected: 0, editor.Workflow.Count);
            editor.CreateGraphNode(new UnitBuilder(), default, (CreateGraphNodeType)nodeType, branch);
            Assert.AreEqual(expected: 1, editor.Workflow.Count);
            assertIsReversible();
        }

        [DataTestMethod]
        [DataRow(CreateGraphNodeType.Successor, false)]
        [DataRow(CreateGraphNodeType.Successor, true)]
        [DataRow(CreateGraphNodeType.Predecessor, false)]
        [DataRow(CreateGraphNodeType.Predecessor, true)]
        public void CreateGraphNode_SingleSelectedNode_ChainNode(object nodeType, bool branch)
        {
            var workflow = new ExpressionBuilderGraph();
            var target = workflow.Add(new UnitBuilder());
            var (editor, assertIsReversible) = CreateMockEditor(workflow);
            Assert.AreEqual(expected: 1, editor.Workflow.Count);
            var selectedNode = editor.FindGraphNode(target.Value);
            var createNodeType = (CreateGraphNodeType)nodeType;
            editor.CreateGraphNode(new UnitBuilder(), selectedNode, createNodeType, branch);
            Assert.AreEqual(expected: 2, editor.Workflow.Count);
            var expectedTargetIndex = createNodeType == CreateGraphNodeType.Successor ? 0 : 1;
            Assert.AreEqual(expected: expectedTargetIndex, editor.Workflow.IndexOf(target));
            assertIsReversible();
        }

        [TestMethod]
        public void ReorderGraphNode_DanglingBranchWithPredecessors_KeepPredecessorEdges()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("ReorderDanglingBranchWithPredecessors.bonsai");
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
            var (editor, assertIsReversible) = CreateMockEditor(workflowBuilder.Workflow);

            // reorder D onto C
            var sourceNode = editor.Workflow[3];
            var targetNode = editor.Workflow[2];
            var nodes = new[] { editor.FindGraphNode(sourceNode.Value) };
            var target = editor.FindGraphNode(targetNode.Value);
            editor.ReorderGraphNodes(nodes, target);

            Assert.AreEqual(expected: editor.Workflow.Count - 1, editor.Workflow.IndexOf(targetNode));
            assertIsReversible();
        }

        [TestMethod]
        public void ConnectGraphNode_ComponentWithHigherIndexIntoLowerIndex_ReorderComponentNodes()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("ConnectComponentWithHigherIndexIntoLowerIndex.bonsai");
            var (editor, assertIsReversible) = CreateMockEditor(workflowBuilder.Workflow);

            // connect D onto C
            var sourceNode = editor.Workflow[3];
            var targetNode = editor.Workflow[2];
            var nodes = new[] { editor.FindGraphNode(sourceNode.Value) };
            var target = editor.FindGraphNode(targetNode.Value);

            editor.ConnectGraphNodes(nodes, target);
            Assert.AreEqual(expected: editor.Workflow.Count - 1, editor.Workflow.IndexOf(targetNode));

            // ensure disconnect is consistent
            editor.DisconnectGraphNodes(nodes, target);
            Assert.AreEqual(expected: editor.Workflow.Count - 1, editor.Workflow.IndexOf(sourceNode));

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
