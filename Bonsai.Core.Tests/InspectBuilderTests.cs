using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bonsai.Expressions;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using Bonsai.Dag;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class InspectBuilderTests
    {
        void RunInspector(params ExpressionBuilder[] builders)
        {
            var workflowBuilder = new WorkflowBuilder();
            var previous = default(Node<ExpressionBuilder, ExpressionBuilderArgument>);
            foreach (var builder in builders)
            {
                var inspector = new InspectBuilder(builder);
                var node = workflowBuilder.Workflow.Add(inspector);
                if (previous != null)
                {
                    workflowBuilder.Workflow.AddEdge(previous, node, new ExpressionBuilderArgument());
                }
                previous = node;
            }
            var result = workflowBuilder.Workflow.BuildObservable();
            var errors = workflowBuilder.Workflow.InspectErrorsEx();
            using (var error = errors.Subscribe(ex => { throw ex; }))
            using (var subscription = result.Subscribe())
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_ExpressionBuilderError_ThrowsBuildException()
        {
            var builder = new ErrorBuilder();
            RunInspector(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowRuntimeException))]
        public void Build_SourceGenerateError_ThrowsException()
        {
            var source = new ErrorSource();
            var builder = new CombinatorBuilder { Combinator = source };
            RunInspector(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowRuntimeException))]
        public void Build_SourceSubscribeError_ThrowsRuntimeException()
        {
            var source = new SubscribeErrorSource();
            var builder = new CombinatorBuilder { Combinator = source };
            RunInspector(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowRuntimeException))]
        public void Build_SourceSubscribeDisposeError_ThrowsRuntimeException()
        {
            var source = new SubscribeDisposeErrorSource();
            var builder = new CombinatorBuilder { Combinator = source };
            RunInspector(builder);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowRuntimeException))]
        public void Build_CombinatorTransformError_ThrowsRuntimeException()
        {
            var source = new UnitBuilder();
            var combinator = new TransformErrorCombinator();
            var builder = new CombinatorBuilder { Combinator = combinator };
            RunInspector(source, builder);
        }

        [TestMethod]
        public void Build_GroupInspectBuilder_ReturnNestedVisualizerElement()
        {
            var workflowBuilder = new WorkflowBuilder();
            var source = workflowBuilder.Workflow.Add(new UnitBuilder());
            var group = workflowBuilder.Workflow.Add(new GroupWorkflowBuilder());
            var groupBuilder = (GroupWorkflowBuilder)group.Value;
            workflowBuilder.Workflow.AddEdge(source, group, new ExpressionBuilderArgument());

            var input = groupBuilder.Workflow.Add(new WorkflowInputBuilder());
            var combinator = groupBuilder.Workflow.Add(new UnitBuilder());
            var output = groupBuilder.Workflow.Add(new WorkflowOutputBuilder());
            groupBuilder.Workflow.AddEdge(input, combinator, new ExpressionBuilderArgument());
            groupBuilder.Workflow.AddEdge(combinator, output, new ExpressionBuilderArgument());

            var inspectable = workflowBuilder.Workflow.ToInspectableGraph();
            var inspectGroup = (InspectBuilder)inspectable.ElementAt(1).Value;
            var result = inspectable.Build();
            var visualizerElement = ExpressionBuilder.GetVisualizerElement(inspectGroup);
            Assert.AreEqual(combinator.Value, visualizerElement.Builder);
        }

        #region Error Classes

        class ErrorBuilder : ExpressionBuilder
        {
            public override Range<int> ArgumentRange
            {
                get { return Range.Create(0, int.MaxValue); }
            }

            public override Expression Build(IEnumerable<Expression> arguments)
            {
                throw new NotImplementedException();
            }
        }

        class ErrorSource : Source<long>
        {
            public override IObservable<long> Generate()
            {
                throw new NotImplementedException();
            }
        }

        class SubscribeErrorSource : Source<long>
        {
            public override IObservable<long> Generate()
            {
                return Observable.Create<long>((Func<IObserver<long>, IDisposable>)(observer =>
                {
                    throw new NotImplementedException();
                }));
            }
        }

        class SubscribeDisposeErrorSource : Source<long>
        {
            public override IObservable<long> Generate()
            {
                return Observable.Create<long>(observer =>
                {
                    return Disposable.Create(() =>
                    {
                        throw new NotImplementedException();
                    });
                });
            }
        }

        class TransformErrorCombinator : Combinator
        {
            public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
            {
                return source.Select<TSource, TSource>(x =>
                {
                    throw new NotImplementedException();
                });
            }
        }

        #endregion
    }
}
