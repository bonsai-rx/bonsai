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
            var builder = new TestWorkflow()
                .Append(new BehaviorSubject<IDisposable> { Name = nameof(BehaviorSubject) })
                .ResetCursor()
                .AppendCombinator(new DoubleProperty { Value = 5.5 })
                .AppendCombinator(new TypeCombinatorMock<IComparable>())
                .Append(new MulticastSubject { Name = nameof(BehaviorSubject) });
            var expression = builder.Workflow.Build();
            Assert.IsNotNull(expression);
        }

        [TestMethod]
        public void Build_MulticastSourceToObjectSubject_PreservesTypeOfSourceSequence()
        {
            // related to https://github.com/bonsai-rx/bonsai/issues/1914
            var workflow = new TestWorkflow()
                .Append(new BehaviorSubject<object> { Name = nameof(BehaviorSubject) })
                .ResetCursor()
                .AppendCombinator(new IntProperty())
                .Append(new MulticastSubject { Name = nameof(BehaviorSubject) })
                .AppendOutput()
                .Workflow;
            var expression = workflow.Build();
            Assert.AreEqual(typeof(IObservable<int>), expression.Type);
        }

        [TestMethod]
        public void ResourceSubject_SourceTerminatesExceptionally_ShouldNotTryToDispose()
        {
            var workflowBuilder = new TestWorkflow()
                .AppendCombinator(new ThrowSource())
                .Append(new ResourceSubject { Name = nameof(ResourceSubject) })
                .AppendCombinator(new CatchSink());
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
                return source.Catch(Observable.Empty<TSource>());
            }
        }

        class SubjectException : Exception
        {
        }
    }
}
