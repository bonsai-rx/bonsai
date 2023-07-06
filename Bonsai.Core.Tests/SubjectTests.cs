using System;
using System.Reactive.Linq;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class SubjectTests
    {
        [Combinator]
        class TypeCombinatorMock<T> : Combinator<T, T>
        {
            public override IObservable<T> Process(IObservable<T> source)
            {
                return source;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_MulticastInterfaceToSubjectOfDifferentInterface_ThrowsBuildException()
        {
            var builder = new WorkflowBuilder();
            builder.Workflow.Add(new BehaviorSubject<IDisposable> { Name = nameof(BehaviorSubject) });
            var source = builder.Workflow.Add(new CombinatorBuilder { Combinator = new DoubleProperty { Value = 5.5 } });
            var convert1 = builder.Workflow.Add(new CombinatorBuilder { Combinator = new TypeCombinatorMock<IComparable>() });
            var convert2 = builder.Workflow.Add(new MulticastSubject { Name = nameof(BehaviorSubject) });
            builder.Workflow.AddEdge(source, convert1, new ExpressionBuilderArgument());
            builder.Workflow.AddEdge(convert1, convert2, new ExpressionBuilderArgument());
            var expression = builder.Workflow.Build();
            Assert.IsNotNull(expression);
        }

        [TestMethod]
        public void ResourceSubject_SourceTerminatesExceptionally_ShouldNotTryToDispose()
        {
            var workflowBuilder = new WorkflowBuilder();
            var source = workflowBuilder.Workflow.Add(new CombinatorBuilder { Combinator = new ThrowSource() });
            var subject = workflowBuilder.Workflow.Add(new ResourceSubject { Name = nameof(ResourceSubject) });
            var sink = workflowBuilder.Workflow.Add(new CombinatorBuilder { Combinator = new CatchSink() });
            workflowBuilder.Workflow.AddEdge(source, subject, new ExpressionBuilderArgument());
            workflowBuilder.Workflow.AddEdge(subject, sink, new ExpressionBuilderArgument());
            var observable = workflowBuilder.Workflow.BuildObservable();
            observable.FirstOrDefaultAsync().Wait();
        }

        class ThrowSource : Source<IDisposable>
        {
            public override IObservable<IDisposable> Generate()
            {
                return Observable.Throw<IDisposable>(new SubjectException());
            }
        }

        class CatchSink : Sink
        {
            public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
            {
                return source.Catch<TSource>(Observable.Empty<TSource>());
            }
        }

        class SubjectException : Exception
        {
        }
    }
}
