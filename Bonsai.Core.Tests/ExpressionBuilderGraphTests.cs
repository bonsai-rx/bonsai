using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class ExpressionBuilderGraphTests
    {
        [TestMethod]
        public void Build_SingleSource_ReturnsExpression()
        {
            var workflow = new ExpressionBuilderGraph();
            workflow.Add(new UnitBuilder());
            var expression = workflow.Build();
            Assert.AreEqual(expected: typeof(IObservable<Unit>), expression.Type);
        }

        [TestMethod]
        public void Build_SimpleGroup_ReturnsExpression()
        {
            var workflow = new ExpressionBuilderGraph();
            workflow.Add(new GroupWorkflowBuilder());
            var expression = workflow.Build();
            Assert.AreEqual(expected: typeof(IObservable<Unit>), expression.Type);
        }

        [TestMethod]
        public void Build_SimpleInspectBuilderGroup_ReturnsExpression()
        {
            var workflow = new ExpressionBuilderGraph();
            workflow.Add(new InspectBuilder(new GroupWorkflowBuilder()));
            var expression = workflow.Build();
            Assert.AreEqual(expected: typeof(IObservable<Unit>), expression.Type);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_CyclicGraph_WorkflowBuildException()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            workflow.AddEdge(source, source, index: 0);
            workflow.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_CyclicalDependency_WorkflowBuildException()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new SubscribeSubject { Name = nameof(SubjectBuilder) });
            var sink = workflow.Add(new PublishSubject { Name = nameof(SubjectBuilder) });
            workflow.AddEdge(source, sink, index: 0);
            workflow.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_NullArgumentRange_WorkflowBuildException()
        {
            var workflow = new ExpressionBuilderGraph();
            workflow.Add(new NullArgumentRangeBuilder());
            workflow.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_BelowArgumentRange_WorkflowBuildException()
        {
            var workflow = new ExpressionBuilderGraph();
            workflow.Add(new WorkflowOutputBuilder());
            workflow.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_AboveArgumentRange_WorkflowBuildException()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            var sink = workflow.Add(new CombinatorBuilder { Combinator = new Timer() });
            workflow.AddEdge(source, sink, index: 0);
            workflow.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_WorkflowOutputIsNotTerminalNode_WorkflowBuildException()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            var sink = workflow.Add(new WorkflowOutputBuilder());
            var appendix = workflow.Add(new UnitBuilder());
            workflow.AddEdge(source, sink, index: 0);
            workflow.AddEdge(sink, appendix, index: 0);
            workflow.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_MultipleWorkflowOutputs_WorkflowBuildException()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            var sink = workflow.Add(new WorkflowOutputBuilder());
            var source2 = workflow.Add(new UnitBuilder());
            var sink2 = workflow.Add(new WorkflowOutputBuilder());
            workflow.AddEdge(source, sink, index: 0);
            workflow.AddEdge(source2, sink2, index: 0);
            workflow.Build();
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_MultipleWorkflowOutputBranch_WorkflowBuildException()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            var sink = workflow.Add(new WorkflowOutputBuilder());
            var sink2 = workflow.Add(new WorkflowOutputBuilder());
            workflow.AddEdge(source, sink, index: 0);
            workflow.AddEdge(source, sink2, index: 0);
            workflow.Build();
        }

        [TestMethod]
        public void Build_HasBuildTarget_ReturnsBuildTargetExpression()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            var sink = workflow.Add(new CombinatorBuilder { Combinator = new IntProperty() });
            workflow.AddEdge(source, sink, index: 0);
            var expression = workflow.Build(source.Value);
            Assert.AreEqual(expected: typeof(IObservable<Unit>), expression.Type);
        }

        [TestMethod]
        public void Build_WithBuildTargetBeforeVisualizerMapping_ReturnsExpression()
        {
            // related to https://github.com/bonsai-rx/bonsai/issues/1591
            var workflow = new TestWorkflow()
                .AppendBranch(root => root
                    .AppendValue(0)
                    .Append(new VisualizerMappingBuilder())
                    .AppendValue(1)
                    .AddArguments(root.AppendUnit()))
                .ToInspectableGraph();
            var buildTarget = workflow[workflow.Count - 1].Value;
            var partialBuild = workflow.Build(buildTarget);
            Assert.IsNotNull(workflow.Build());
        }

        [TestMethod]
        public void Build_ActiveBranch_HasMulticastExpression()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            var sink = workflow.Add(new UnitBuilder());
            var sink2 = workflow.Add(new UnitBuilder());
            workflow.AddEdge(source, sink, index: 0);
            workflow.AddEdge(source, sink2, index: 0);
            var expression = workflow.Build();
            var visitor = new PublishBranchVisitor();
            visitor.Visit(expression);
            Assert.IsTrue(visitor.HasPublishBranch);
        }

        [TestMethod]
        public void Build_DisabledBranch_AvoidMulticastExpression()
        {
            var workflow = new TestWorkflow()
                .AppendUnit()
                .AppendBranch(source => source
                    .AppendCombinator(new IntProperty())
                    .ResetCursor(source.Cursor)
                    .Append(new DisableBuilder(new UnitBuilder())))
                .Workflow;
            var expression = workflow.Build();
            var visitor = new PublishBranchVisitor();
            visitor.Visit(expression);
            Assert.IsFalse(visitor.HasPublishBranch);
        }

        [TestMethod]
        public void Build_DisableAllBranches_ReturnSourceExpression()
        {
            var workflow = new TestWorkflow()
                .AppendUnit()
                .AppendBranch(source => source
                    .Append(new DisableBuilder(new FormatBuilder()))
                    .ResetCursor(source.Cursor)
                    .Append(new DisableBuilder(new FormatBuilder())))
                .Workflow;
            var expression = workflow.Build();
            var visitor = new MergeBranchVisitor();
            visitor.Visit(expression);
            Assert.AreEqual(expected: 0, visitor.BranchCount);
            Assert.AreEqual(expected: typeof(IObservable<Unit>), expression.Type);
        }

        [TestMethod]
        public void Build_DisableCombinatorInChain_KeepSourceExpression()
        {
            var workflow = new TestWorkflow()
                .AppendValue(0)
                .Append(new DisableBuilder(new UnitBuilder()))
                .Append(new MemberSelectorBuilder())
                .Workflow;

            var expression = workflow.Build();
            Assert.IsNotNull(expression);
        }

        [TestMethod]
        public void Build_DisableDanglingBranchParallelWithMerge_KeepMulticastBranch()
        {
            // related to https://github.com/bonsai-rx/bonsai/issues/2007
            var workflow = new TestWorkflow()
                .AppendValue(0)
                .AppendBranch(source => source
                    .AppendUnit()
                    .AppendCombinator(new Merge())
                    .AddArguments(source.AppendUnit())
                    .ResetCursor(source.Cursor)
                    .Append(new DisableBuilder(new UnitBuilder())))
                .Workflow;

            var expression = workflow.Build();
            var visitor = new PublishBranchVisitor();
            visitor.Visit(expression);
            Assert.IsTrue(visitor.HasPublishBranch);
        }

        [TestMethod]
        public void Build_DisabledSource_ReturnsEmptyExpression()
        {
            var workflow = new ExpressionBuilderGraph();
            workflow.Add(new DisableBuilder(new UnitBuilder()));
            var expression = workflow.Build();
            var visitor = new MergeBranchVisitor();
            visitor.Visit(expression);
            Assert.AreEqual(expected: 0, visitor.BranchCount);
        }

        [TestMethod]
        public void Build_DisabledPropertyMapping_DisconnectedBranch()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            var source2 = workflow.Add(new UnitBuilder());
            var mapping = workflow.Add(new DisableBuilder(new PropertyMappingBuilder()));
            workflow.AddEdge(source2, mapping, index: 0);
            workflow.AddEdge(mapping, source, index: 0);
            var expression = workflow.Build();
            var visitor = new MergeBranchVisitor();
            visitor.Visit(expression);
            Assert.AreEqual(expected: 2, visitor.BranchCount);
        }

        [TestMethod]
        public void Build_BranchPropertyMapping_AvoidMulticastExpression()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            var mapping = workflow.Add(new PropertyMappingBuilder());
            var sink = workflow.Add(new UnitBuilder());
            var sink2 = workflow.Add(new UnitBuilder());
            workflow.AddEdge(source, mapping, index: 0);
            workflow.AddEdge(mapping, sink, index: 0);
            workflow.AddEdge(mapping, sink2, index: 0);
            var expression = workflow.Build();
            var visitor = new PublishBranchVisitor();
            visitor.Visit(expression);
            Assert.IsFalse(visitor.HasPublishBranch);
        }

        [TestMethod]
        public void Build_PropertyMappingToHiddenProperty_PreferDerivedProperty()
        {
            new TestWorkflow()
                .AppendValue(1)
                .AppendPropertyMapping(nameof(DerivedValueProperty.Value))
                .AppendCombinator(new DerivedValueProperty())
                .BuildObservable<Unit>();
        }

        [TestMethod]
        public void Build_PropertyMappingToInheritedHiddenProperty_PreferDerivedProperty()
        {
            new TestWorkflow()
                .AppendValue(1)
                .AppendPropertyMapping(nameof(DerivedValueProperty.Value))
                .AppendCombinator(new DerivedNewProperty())
                .BuildObservable<Unit>();
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_PropertyMappingToMissingProperty_ThrowsWorkflowBuildException()
        {
            new TestWorkflow()
                .AppendValue(1)
                .AppendPropertyMapping(nameof(DerivedNewProperty.AnotherValue))
                .AppendCombinator(new DerivedValueProperty())
                .BuildObservable<Unit>();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BuildObservable_InvalidWorkflowType_ThrowsInvalidOperationException()
        {
            new TestWorkflow()
                .AppendValue(0)
                .AppendOutput()
                .BuildObservable<Unit>();
        }

        [TestMethod]
        public void BuildObservable_CovariantWorkflowType_IsCompatibleAssignment()
        {
            var workflow = new TestWorkflow()
                .AppendValue("")
                .AppendOutput()
                .BuildObservable<object>();
            Assert.IsNotNull(workflow);
        }

        [TestMethod]
        public void BuildObservable_ConvertibleWorkflowType_IsCompatibleAssignment()
        {
            var workflow = new TestWorkflow()
                .AppendValue(1)
                .AppendOutput()
                .BuildObservable<double>();
            Assert.IsNotNull(workflow);
        }

        class DerivedValueProperty : WorkflowProperty<int>
        {
            [Range(0, 100)]
            public new int Value { get; set; }
        }

        class DerivedNewProperty : DerivedValueProperty
        {
            public int AnotherValue { get; set; }
        }

        class MergeBranchVisitor : ExpressionVisitor
        {
            public int BranchCount { get; private set; }

            protected override Expression VisitNewArray(NewArrayExpression node)
            {
                BranchCount = node.Expressions.Count;
                return base.VisitNewArray(node);
            }
        }

        class PublishBranchVisitor : ExpressionVisitor
        {
            public bool HasPublishBranch { get; private set; }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method?.Name == "Multicast")
                {
                    HasPublishBranch = (node.Object?.Type.Name.Contains("Publish"))
                        .GetValueOrDefault();
                }

                return base.VisitMethodCall(node);
            }
        }

        class NullArgumentRangeBuilder : ExpressionBuilder
        {
            public override Range<int> ArgumentRange => null;

            public override Expression Build(IEnumerable<Expression> arguments)
            {
                throw new NotImplementedException();
            }
        }
    }
}
