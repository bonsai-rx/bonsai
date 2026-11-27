using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Dag;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class WorkflowTraversalTests
    {
        const string SubjectName = "Value";
        const string IncludedWorkflowPath = "Bonsai.Core.Tests:LookupNodesIncluded.bonsai";

        static ExpressionBuilder[] GetBuilders(IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> nodes)
        {
            return nodes.Select(node => node.Value).ToArray();
        }

        static IEnumerable<(ExpressionBuilderGraph Workflow, InspectBuilder Reference)> FindSubjectReferences(ExpressionBuilderGraph workflow)
        {
            foreach (var node in workflow)
            {
                var inspectBuilder = (InspectBuilder)node.Value;
                if (inspectBuilder.Builder is SubscribeSubject)
                    yield return (workflow, inspectBuilder);

                if (inspectBuilder.Builder is IWorkflowExpressionBuilder workflowBuilder && workflowBuilder.Workflow is not null)
                {
                    foreach (var reference in FindSubjectReferences(workflowBuilder.Workflow))
                        yield return reference;
                }
            }
        }

        static void AssertLookupMatchesBuild(TestWorkflow workflow)
        {
            var root = workflow.ToInspectableGraph();
            root.Build();

            var references = FindSubjectReferences(root).ToArray();
            Assert.AreNotEqual(0, references.Length);
            foreach (var (scope, reference) in references)
            {
                var name = ((SubscribeSubject)reference.Builder).Name;
                var declaration = new WorkflowBuilder(root).LookupNodes(scope)
                    .SelectMany(level => level)
                    .Select(node => (InspectBuilder)node.Value)
                    .First(builder => builder.Builder is SubjectExpressionBuilder subject && subject.Name == name);
                Assert.AreEqual(declaration.ObservableType, reference.ObservableType);
            }
        }

        [TestMethod]
        public void LocalNodes_GroupNode_ReplacedByNestedLocalNodes()
        {
            ExpressionBuilder nested = null;
            var workflow = new TestWorkflow()
                .AppendUnit()
                .Capture(out var first)
                .ResetCursor()
                .AppendNested(
                    group => group.AppendUnit().Capture(out nested),
                    workflow => new GroupWorkflowBuilder(workflow))
                .ResetCursor()
                .AppendUnit()
                .Capture(out var last);
            var result = GetBuilders(workflow.Workflow.LocalNodes());
            CollectionAssert.AreEqual(new[] { first, nested, last }, result);
        }

        [TestMethod]
        public void LocalNodes_IncludeNode_ReplacedByIncludedLocalNodes()
        {
            var includeBuilder = new IncludeWorkflowBuilder { Path = IncludedWorkflowPath };
            var workflow = new TestWorkflow()
                .Append(includeBuilder)
                .Workflow;
            workflow.Build();
            var result = GetBuilders(workflow.LocalNodes());
            CollectionAssert.AreEqual(GetBuilders(includeBuilder.Workflow), result);
        }

        [TestMethod]
        public void LocalNodes_UnloadedIncludeNode_ContributesNoNodes()
        {
            var workflow = new TestWorkflow()
                .Append(new IncludeWorkflowBuilder { Path = IncludedWorkflowPath })
                .Workflow;
            var result = workflow.LocalNodes();
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void LocalNodes_DisabledNode_Skipped()
        {
            var workflow = new TestWorkflow()
                .AppendUnit()
                .Capture(out var enabled)
                .ResetCursor()
                .Append(new DisableBuilder(new UnitBuilder()))
                .ResetCursor()
                .Append(new DisableBuilder(new GroupWorkflowBuilder(new TestWorkflow().AppendUnit().Workflow)));
            var result = GetBuilders(workflow.Workflow.LocalNodes());
            CollectionAssert.AreEqual(new[] { enabled }, result);
        }

        [TestMethod]
        public void LocalNodes_NestedWorkflow_NotExpanded()
        {
            var workflow = new TestWorkflow()
                .AppendUnit()
                .Capture(out var source)
                .AppendNested(
                    input => input.AppendOutput(),
                    workflow => new SelectMany(workflow))
                .Capture(out var selectMany);
            var result = GetBuilders(workflow.Workflow.LocalNodes());
            CollectionAssert.AreEqual(new[] { source, selectMany }, result);
        }

        [TestMethod]
        public void LookupNodes_NestedWorkflow_ReturnsInnermostLocalNodesFirst()
        {
            var nestedWorkflow = new TestWorkflow().AppendUnit().Workflow;
            var workflow = new TestWorkflow()
                .AppendUnit()
                .Append(new SelectMany(nestedWorkflow))
                .Workflow;
            var workflowBuilder = new WorkflowBuilder(workflow);
            var result = GetBuilders(workflowBuilder.LookupNodes(nestedWorkflow).SelectMany(level => level));
            var expected = GetBuilders(nestedWorkflow.LocalNodes().Concat(workflow.LocalNodes()));
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void LookupNodes_NestedWorkflow_ReturnsWorkflowContainerGroups()
        {
            var nestedWorkflow = new TestWorkflow().AppendUnit().Workflow;
            var selectMany = new SelectMany(nestedWorkflow);
            var workflow = new TestWorkflow()
                .AppendUnit()
                .Append(selectMany)
                .Workflow;
            var workflowBuilder = new WorkflowBuilder(workflow);
            var keys = workflowBuilder.LookupNodes(nestedWorkflow).Select(level => level.Key).ToArray();
            CollectionAssert.AreEqual(new IWorkflowBuilder[] { selectMany, workflowBuilder }, keys);
        }

        [TestMethod]
        public void LookupNodes_GroupWorkflowTarget_ReturnsEnclosingLocalNodes()
        {
            var groupWorkflow = new TestWorkflow().AppendUnit().Workflow;
            var workflow = new TestWorkflow()
                .AppendUnit()
                .ResetCursor()
                .Append(new GroupWorkflowBuilder(groupWorkflow))
                .Workflow;
            var workflowBuilder = new WorkflowBuilder(workflow);
            var levels = workflowBuilder.LookupNodes(groupWorkflow).ToArray();
            Assert.AreEqual(1, levels.Length);
            Assert.AreSame(workflowBuilder, levels[0].Key);
            CollectionAssert.AreEqual(GetBuilders(workflow.LocalNodes()), GetBuilders(levels[0]));
        }

        [TestMethod]
        public void LookupNodes_UnreachableTarget_ReturnsEmpty()
        {
            var workflowBuilder = new WorkflowBuilder(new TestWorkflow().AppendUnit().Workflow);
            var result = workflowBuilder.LookupNodes(new ExpressionBuilderGraph());
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public void LookupNodes_DisabledNestedTarget_ReturnsEmpty()
        {
            var nestedWorkflow = new TestWorkflow().AppendUnit().Workflow;
            var workflow = new TestWorkflow()
                .Append(new DisableBuilder(new SelectMany(nestedWorkflow)))
                .Workflow;
            var result = new WorkflowBuilder(workflow).LookupNodes(nestedWorkflow);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LookupNodes_NullTarget_ThrowsArgumentNullExceptionOnCall()
        {
            var workflowBuilder = new WorkflowBuilder();
            workflowBuilder.LookupNodes(null);
        }

        [TestMethod]
        public void LookupNodes_NestedDeclaration_MatchesBuildResolution()
        {
            // A declaration in a nested workflow shadows the outer declaration of the same name
            var workflow = new TestWorkflow()
                .AppendValue(0)
                .AppendSubject<BehaviorSubject>(SubjectName)
                .ResetCursor()
                .Append(new SubscribeSubject { Name = SubjectName })
                .AppendNested(
                    input => input
                        .ResetCursor()
                        .AppendValue("value")
                        .AppendSubject<BehaviorSubject>(SubjectName)
                        .ResetCursor()
                        .Append(new SubscribeSubject { Name = SubjectName })
                        .AppendOutput(),
                    workflow => new SelectMany(workflow));
            AssertLookupMatchesBuild(workflow);
        }

        [TestMethod]
        public void LookupNodes_GroupDeclaration_MatchesBuildResolution()
        {
            // A declaration in a group can be referred to from the workflow containing the group
            var workflow = new TestWorkflow()
                .AppendNested(
                    group => group
                        .AppendValue("value")
                        .AppendSubject<BehaviorSubject>(SubjectName),
                    workflow => new GroupWorkflowBuilder(workflow))
                .ResetCursor()
                .Append(new SubscribeSubject { Name = SubjectName })
                .AppendNested(
                    input => input
                        .ResetCursor()
                        .Append(new SubscribeSubject { Name = SubjectName })
                        .AppendOutput(),
                    workflow => new SelectMany(workflow));
            AssertLookupMatchesBuild(workflow);
        }

        [TestMethod]
        public void LookupNodes_IncludeDeclaration_MatchesBuildResolution()
        {
            // A declaration in an included workflow can be referred to from the including workflow
            var workflow = new TestWorkflow()
                .Append(new IncludeWorkflowBuilder { Path = IncludedWorkflowPath })
                .ResetCursor()
                .Append(new SubscribeSubject { Name = SubjectName });
            AssertLookupMatchesBuild(workflow);
        }

        [TestMethod]
        public void LookupNodes_SiblingDeclarations_MatchBuildResolution()
        {
            // Declarations with the same name in sibling nested workflows are independent
            var workflow = new TestWorkflow()
                .AppendValue(0)
                .AppendNested(
                    input => input
                        .ResetCursor()
                        .AppendValue("value")
                        .AppendSubject<BehaviorSubject>(SubjectName)
                        .ResetCursor()
                        .Append(new SubscribeSubject { Name = SubjectName })
                        .AppendOutput(),
                    workflow => new SelectMany(workflow))
                .ResetCursor()
                .AppendValue(0)
                .AppendNested(
                    input => input
                        .ResetCursor()
                        .AppendValue(0.0)
                        .AppendSubject<BehaviorSubject>(SubjectName)
                        .ResetCursor()
                        .Append(new SubscribeSubject { Name = SubjectName })
                        .AppendOutput(),
                    workflow => new SelectMany(workflow));
            AssertLookupMatchesBuild(workflow);
        }
    }
}
