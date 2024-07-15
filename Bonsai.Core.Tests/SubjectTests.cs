using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
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

        class ConstantExpressionBuilder : ZeroArgumentExpressionBuilder
        {
            public Expression Expression { get; set; }

            public override Expression Build(IEnumerable<Expression> arguments)
            {
                return Expression;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Build_MulticastSubjectMissingBuildContext_ThrowsBuildException()
        {
            var source = new UnitBuilder().Build();
            var builder = new MulticastSubject { Name = nameof(BehaviorSubject) };
            builder.Build(source);
            Assert.Fail();
        }

        [TestMethod]
        public void Build_MulticastSubjectMissingName_ReturnsSameSequence()
        {
            var source = Expression.Constant(Observable.Return(0));
            var builder = new TestWorkflow()
                .Append(new ConstantExpressionBuilder { Expression = source })
                .Append(new MulticastSubject())
                .AppendOutput();
            var expression = builder.Workflow.Build();
            Assert.AreSame(source, expression);
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
        public async Task Build_MulticastSourceToSubject_ReturnsSameValue()
        {
            var value = 32;
            var workflow = new TestWorkflow()
                .Append(new BehaviorSubject<int> { Name = nameof(BehaviorSubject) })
                .ResetCursor()
                .AppendCombinator(new IntProperty { Value = value })
                .Append(new MulticastSubject { Name = nameof(BehaviorSubject) })
                .AppendOutput();
            var observable = workflow.BuildObservable<int>();
            Assert.AreEqual(value, await observable.Take(1));
        }

        [TestMethod]
        public async Task Build_MulticastSourceToObjectSubject_PreservesTypeOfSourceSequence()
        {
            // related to https://github.com/bonsai-rx/bonsai/issues/1914
            var workflow = new TestWorkflow()
                .Append(new BehaviorSubject<object> { Name = nameof(BehaviorSubject) })
                .ResetCursor()
                .AppendCombinator(new IntProperty())
                .Append(new MulticastSubject { Name = nameof(BehaviorSubject) })
                .AppendOutput();
            var observable = workflow.BuildObservable<int>();
            Assert.AreEqual(0, await observable.Take(1));
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
