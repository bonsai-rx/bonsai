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
        static void RunInspector(params ExpressionBuilder[] builders)
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
            ExpressionBuilder target = null;
            var workflow = TestWorkflow
                .New()
                .AppendUnit()
                .AppendNested(
                    input => input
                        .AppendUnit()
                        .Capture(out target)
                        .AppendOutput(),
                    workflow => new GroupWorkflowBuilder(workflow))
                .ToInspectableGraph();
            workflow.Build();

            var output = workflow[workflow.Count - 1].Value;
            var visualizerElement = ExpressionBuilder.GetVisualizerElement(output);
            Assert.AreSame(target, visualizerElement.Builder);
        }

        [TestMethod]
        public void Build_PropertyMappedInspectBuilderToWorkflowOutput_ReturnVisualizerElement()
        {
            ExpressionBuilder target;
            var workflow = TestWorkflow
                .New()
                .AppendValue(0)
                .AppendPropertyMapping(nameof(Reactive.Range.Count))
                .AppendCombinator(new Reactive.Range())
                .Capture(out target)
                .AppendOutput()
                .ToInspectableGraph();
            workflow.Build();

            var output = workflow[workflow.Count - 1].Value;
            var visualizerElement = ExpressionBuilder.GetVisualizerElement(output);
            Assert.AreSame(target, visualizerElement.Builder);
        }

        [TestMethod]
        public void Build_SinkInspectBuilder_ReturnSourceVisualizerElement()
        {
            var workflow = TestWorkflow
                .New()
                .AppendValue(1)
                .AppendNested(
                    input => input
                        .AppendValue(string.Empty)
                        .AppendOutput(),
                    workflow => new Reactive.Sink(workflow))
                .AppendOutput()
                .ToInspectableGraph();
            workflow.Build();

            var sourceVisualizer = ExpressionBuilder.GetVisualizerElement(workflow[0].Value);
            var outputVisualizer = ExpressionBuilder.GetVisualizerElement(workflow[workflow.Count - 1].Value);
            Assert.AreSame(sourceVisualizer, outputVisualizer);
        }

        [TestMethod]
        public void Build_VisualizerInspectBuilder_ReplaceSourceVisualizerElement()
        {
            var workflow = TestWorkflow
                .New()
                .AppendValue(1)
                .AppendNested(
                    input => input
                        .AppendValue(string.Empty)
                        .AppendOutput(),
                    workflow => new Reactive.Visualizer(workflow))
                .AppendOutput()
                .ToInspectableGraph();
            workflow.Build();

            var sourceBuilder = (InspectBuilder)workflow[0].Value;
            var outputBuilder = (InspectBuilder)workflow[workflow.Count - 1].Value;
            Assert.AreEqual(typeof(int), sourceBuilder.ObservableType);
            Assert.AreEqual(sourceBuilder.ObservableType, outputBuilder.ObservableType);

            var sourceVisualizer = ExpressionBuilder.GetVisualizerElement(sourceBuilder);
            var outputVisualizer = ExpressionBuilder.GetVisualizerElement(outputBuilder);
            Assert.AreNotSame(sourceVisualizer, outputVisualizer);
            Assert.AreEqual(typeof(string), outputVisualizer.ObservableType);
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
