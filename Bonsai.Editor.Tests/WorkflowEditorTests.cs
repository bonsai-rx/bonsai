using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Bonsai.Core.Tests;
using Bonsai.Dag;
using Bonsai.Editor.GraphModel;
using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Editor.Tests
{
    [TestClass]
    public partial class WorkflowEditorTests
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
            return ElementStore.LoadWorkflow(reader);
        }

        static void AssertIsSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            expected = expected.ToArray();
            actual = actual.ToArray();
            if (!expected.SequenceEqual(actual))
            {
                static string ToString(IEnumerable<T> sequence)
                {
                    return string.Join(",", sequence);
                }
                var expectedString = ToString(expected);
                var actualString = ToString(actual);
                Assert.Fail($"Sequence is not equal. Expected: {expectedString}. Actual: {actualString}");
            }
        }

        (WorkflowEditor editor, Action assertIsReversible) CreateMockEditor(
            ExpressionBuilderGraph workflow = null,
            MockGraphView graphView = null)
        {
            graphView ??= new MockGraphView(workflow);
            var editor = new WorkflowEditor(graphView.ServiceProvider, graphView);
            editor.UpdateLayout.Subscribe(graphView.UpdateGraphLayout);
            editor.UpdateSelection.Subscribe(graphView.UpdateSelection);

            var nodeSequence = editor.GetGraphValues().ToArray();
            return (editor, assertIsReversible: () =>
            {
                while (graphView.CommandExecutor.CanUndo)
                {
                    graphView.CommandExecutor.Undo();
                }

                AssertIsSequenceEqual(nodeSequence, editor.GetGraphValues());
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
            editor.CreateNode("A", default, (CreateGraphNodeType)nodeType, branch);
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
            var workflow = EditorHelper.CreateEditorGraph("A");
            var (editor, assertIsReversible) = CreateMockEditor(workflow);
            Assert.AreEqual(expected: 1, editor.Workflow.Count);
            var selectedNode = editor.FindNode("A");
            var createNodeType = (CreateGraphNodeType)nodeType;
            editor.CreateNode("B", selectedNode, createNodeType, branch);
            Assert.AreEqual(expected: 2, editor.Workflow.Count);
            var expectedTargetIndex = createNodeType == CreateGraphNodeType.Successor ? 0 : 1;
            Assert.AreEqual(expected: expectedTargetIndex, editor.FindNode("A").Index);
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
            Assert.AreEqual(expected: 4, editor.Workflow.IndexOf(sourceNode));
            assertIsReversible();
        }

        [TestMethod]
        public void ReorderGraphNode_ComponentWithHigherIndexIntoLowerIndex_ReorderComponentNodes()
        {
            var workflowBuilder = LoadEmbeddedWorkflow("ReorderComponentWithHigherIndexIntoLowerIndex.bonsai");
            var (editor, assertIsReversible) = CreateMockEditor(workflowBuilder.Workflow);

            // reorder D onto C
            var sourceNode = editor.Workflow[2];
            var targetNode = editor.Workflow[0];
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

        [TestMethod]
        public void DisableGraphNodes_AllNodesInBranch_KeepNodeIndices()
        {
            var nodes = new[] { "A", "B", "C", "D" };
            var workflow = EditorHelper.CreateEditorGraph(nodes);
            var (editor, assertIsReversible) = CreateMockEditor(workflow);
            var nodeSequence = editor.Workflow.ToArray();
            editor.ConnectNodes("A", "B");
            editor.ConnectNodes("B", "C");
            editor.ConnectNodes("A", "D");
            Assert.AreEqual(expected: nodes.Length, editor.Workflow.Count);
            AssertIsSequenceEqual(nodeSequence, editor.Workflow);

            var nodesToDisable = new[] { editor.FindNode("B"), editor.FindNode("C") };
            editor.DisableGraphNodes(nodesToDisable);
            AssertIsSequenceEqual(nodes, editor.GetGraphValues());
            assertIsReversible();
        }

        [TestMethod]
        public void UngroupGraphNodes_NestedPassthrough_KeepOuterEdges()
        {
            var workflow = EditorHelper.CreateEditorGraph("A", "C");
            var (editor, assertIsReversible) = CreateMockEditor(workflow);
            var sourceNode = editor.FindNode("A");
            editor.CreateGraphNode(
                new GroupWorkflowBuilder { Name = "B" },
                sourceNode,
                CreateGraphNodeType.Successor,
                branch: false);
            editor.ConnectNodes("B", "C");
            var nodesToUngroup = new[] { editor.FindNode("B") };
            editor.UngroupGraphNodes(nodesToUngroup);
            assertIsReversible();
        }

        [TestMethod]
        public void DisconnectGraphNodes_TargetIsNotRoot_InsertAfterClosestRoot()
        {
            var workflow = EditorHelper.CreateEditorGraph("A", "B", "C", "D");
            var (editor, assertIsReversible) = CreateMockEditor(workflow);
            editor.ConnectNodes("A", "C");
            editor.ConnectNodes("B", "C");
            editor.ConnectNodes("C", "D");
            var sourceNode = editor.FindNode("B");
            var targetNode = editor.FindNode("C");
            Assert.AreEqual(expected: editor.Workflow.Count - 1, editor.FindNode("D").Index);
            editor.DisconnectGraphNodes(new[] { sourceNode }, targetNode);
            Assert.AreEqual(expected: editor.Workflow.Count - 1, editor.FindNode("B").Index);
            assertIsReversible();
        }

        [TestMethod]
        public void CreateAnnotation_EmptySelection_InsertAfterClosestRoot()
        {
            var workflow = EditorHelper.CreateEditorGraph("A");
            var (editor, assertIsReversible) = CreateMockEditor(workflow);
            var annotationBuilder = new AnnotationBuilder();
            editor.CreateGraphNode(annotationBuilder, null, CreateGraphNodeType.Successor, branch: false);
            Assert.AreEqual(expected: editor.Workflow.Count - 1, editor.FindNode(annotationBuilder).Index);
            assertIsReversible();
        }

        [TestMethod]
        public void ReplaceGraphNode_SingleInputWithVisualizerMapping_GroupWorkflowHasSingleSourceNode()
        {
            // related to https://github.com/bonsai-rx/bonsai/issues/1792
            var workflow = new TestWorkflow()
                .AppendValue(0)
                .AppendBranch(source => source
                    .AppendSubject<Reactive.PublishSubject>("P")
                    .AddArguments(source.Append(new VisualizerMappingBuilder())))
                .TopologicalSort()
                .ToInspectableGraph();

            var (editor, assertIsReversible) = CreateMockEditor(workflow);
            var targetNode = editor.FindNode("P");
            editor.ReplaceGraphNode(
                targetNode,
                typeof(GroupWorkflowBuilder).AssemblyQualifiedName,
                ElementCategory.Nested,
                arguments: "N");
            Assert.AreEqual(expected: 3, workflow.Count);

            var groupNode = editor.FindNode("N");
            var groupBuilder = ExpressionBuilder.Unwrap(groupNode?.Value) as GroupWorkflowBuilder;
            Assert.IsInstanceOfType(groupBuilder, typeof(GroupWorkflowBuilder));
            Assert.AreEqual(expected: 2, groupBuilder.Workflow.Count);
            Assert.IsInstanceOfType(ExpressionBuilder.Unwrap(groupBuilder.Workflow[0].Value), typeof(WorkflowInputBuilder));
            Assert.IsInstanceOfType(ExpressionBuilder.Unwrap(groupBuilder.Workflow[1].Value), typeof(WorkflowOutputBuilder));
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
