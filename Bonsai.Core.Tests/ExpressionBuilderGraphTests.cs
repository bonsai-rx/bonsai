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
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            var sink = workflow.Add(new CombinatorBuilder { Combinator = new IntProperty() });
            var sink2 = workflow.Add(new DisableBuilder(new UnitBuilder()));
            workflow.AddEdge(source, sink, index: 0);
            workflow.AddEdge(source, sink2, index: 0);
            var expression = workflow.Build();
            var visitor = new PublishBranchVisitor();
            visitor.Visit(expression);
            Assert.IsFalse(visitor.HasPublishBranch);
        }

        [TestMethod]
        public void Build_DisableAllBranches_ReturnSourceExpression()
        {
            var workflow = new ExpressionBuilderGraph();
            var source = workflow.Add(new UnitBuilder());
            var sink = workflow.Add(new DisableBuilder(new FormatBuilder()));
            var sink2 = workflow.Add(new DisableBuilder(new FormatBuilder()));
            workflow.AddEdge(source, sink, index: 0);
            workflow.AddEdge(source, sink2, index: 0);
            var expression = workflow.Build();
            var visitor = new MergeBranchVisitor();
            visitor.Visit(expression);
            Assert.AreEqual(expected: 0, visitor.BranchCount);
            Assert.AreEqual(expected: typeof(IObservable<Unit>), expression.Type);
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
        [ExpectedException(typeof(ArgumentException))]
        public void BuildObservable_InvalidWorkflowType_ThrowsArgumentException()
        {
            new TestWorkflow()
                .AppendValue(0)
                .AppendOutput()
                .BuildObservable<Unit>();
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
