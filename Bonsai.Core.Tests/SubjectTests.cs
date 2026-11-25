using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
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
                .AppendSource(Observable.Return<IComparable>(5.5))
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
                .AppendThrow<IDisposable>(new SubjectException())
                .Append(new ResourceSubject { Name = nameof(ResourceSubject) })
                .AppendCombinator(new CatchSink());
            var observable = workflowBuilder.Workflow.BuildObservable();
            observable.FirstOrDefaultAsync().Wait();
        }

        [TestMethod]
        public async Task PublishSubject_WorkflowCompletes_CompletesActiveObserver()
        {
            // An observer active when the scope ends receives the completion of the subject.
            var capture = new CaptureSequence<int>();
            var workflow = await BuildNestedWorkflow<int>(input => input
                .AppendOutput()
                .ResetCursor()
                .AppendSubject<PublishSubject<int>>(nameof(PublishSubject))
                .ResetCursor()
                .AppendSource(Observable.Return(1).Concat(Observable.Never<int>()))
                .Append(new MulticastSubject { Name = nameof(PublishSubject) })
                .ResetCursor()
                .Append(new SubscribeSubject { Name = nameof(PublishSubject) })
                .AppendCombinator(capture));

            var notifications = Observe(capture.Sequence);
            await workflow;
            CollectionAssert.AreEqual(
                new[] { Notification.CreateOnNext(1), Notification.CreateOnCompleted<int>() },
                notifications);
        }

        [TestMethod]
        public async Task PublishSubject_SubscriptionDisposed_CompletesActiveObserver()
        {
            // Disposing the scope subscription completes the observers of a subject declared as a sink.
            var capture = new CaptureSequence<int>();
            var workflow = await BuildNestedWorkflow<int>(input => input
                .ResetCursor()
                .AppendSource(Observable.Never<int>())
                .AppendOutput()
                .ResetCursor()
                .AppendSource(Observable.Return(1).Concat(Observable.Never<int>()))
                .Append(new PublishSubject { Name = nameof(PublishSubject) })
                .ResetCursor()
                .Append(new SubscribeSubject { Name = nameof(PublishSubject) })
                .AppendCombinator(capture));

            var notifications = Observe(capture.Sequence);
            workflow.Subscribe().Dispose();
            CollectionAssert.AreEqual(
                new[] { Notification.CreateOnNext(1), Notification.CreateOnCompleted<int>() },
                notifications);
        }

        [TestMethod]
        public async Task PublishSubject_WorkflowCompleted_CompletesLateObserver()
        {
            // An observer subscribing after the scope ended receives completion instead of an error.
            var capture = new CaptureSequence<int>();
            var workflow = await BuildNestedWorkflow<int>(input => input
                .AppendOutput()
                .ResetCursor()
                .AppendSubject<PublishSubject<int>>(nameof(PublishSubject))
                .ResetCursor()
                .Append(new SubscribeSubject { Name = nameof(PublishSubject) })
                .AppendCombinator(capture));

            await workflow;
            var notifications = await capture.Sequence.Materialize().ToList();
            CollectionAssert.AreEqual(new[] { Notification.CreateOnCompleted<int>() }, notifications.ToArray());
        }

        [TestMethod]
        public async Task PublishSubject_WriterFaulted_ReportsErrorToLateObserver()
        {
            // A subject terminated with an error before the scope ends keeps reporting that error.
            var capture = new CaptureSequence<int>();
            var workflow = await BuildNestedWorkflow<int>(input => input
                .AppendOutput()
                .ResetCursor()
                .AppendSubject<PublishSubject<int>>(nameof(PublishSubject))
                .ResetCursor()
                .AppendThrow<int>(new SubjectException())
                .Append(new MulticastSubject { Name = nameof(PublishSubject) })
                .ResetCursor()
                .Append(new SubscribeSubject { Name = nameof(PublishSubject) })
                .AppendCombinator(capture));

            var notifications = Observe(capture.Sequence);
            var result = await workflow.Materialize().LastAsync();
            var lateNotifications = await capture.Sequence.Materialize().ToList();
            Assert.IsInstanceOfType(result.Exception, typeof(SubjectException));
            Assert.AreEqual(1, notifications.Count);
            Assert.IsInstanceOfType(notifications[0].Exception, typeof(SubjectException));
            Assert.AreEqual(1, lateNotifications.Count);
            Assert.IsInstanceOfType(lateNotifications[0].Exception, typeof(SubjectException));
        }

        [TestMethod]
        public async Task BehaviorSubject_WriterCompleted_ForwardsLaterValues()
        {
            // Completion of a writer does not complete the subject while the scope is alive.
            var capture = new CaptureSequence<int>();
            var workflow = await BuildBehaviorSubjectWorkflow(capture);
            var notifications = Observe(capture.Sequence);
            await workflow;
            CollectionAssert.AreEqual(
                new[] { Notification.CreateOnNext(1), Notification.CreateOnNext(2) },
                notifications.GetRange(0, 2));
        }

        [TestMethod]
        public async Task BehaviorSubject_WorkflowCompletes_CompletesActiveObserver()
        {
            // The end of the scope completes the observers of a subject that ignores writer completion.
            var capture = new CaptureSequence<int>();
            var workflow = await BuildBehaviorSubjectWorkflow(capture);
            var notifications = Observe(capture.Sequence);
            await workflow;
            CollectionAssert.AreEqual(
                new[] { Notification.CreateOnNext(1), Notification.CreateOnNext(2), Notification.CreateOnCompleted<int>() },
                notifications);
        }

        [TestMethod]
        public async Task BehaviorSubject_WorkflowCompleted_ReplaysLastValueToLateObserver()
        {
            // An observer subscribing after the scope ended receives the last value followed by completion.
            var capture = new CaptureSequence<int>();
            var workflow = await BuildBehaviorSubjectWorkflow(capture);
            await workflow;
            var notifications = await capture.Sequence.Materialize().ToList();
            CollectionAssert.AreEqual(
                new[] { Notification.CreateOnNext(2), Notification.CreateOnCompleted<int>() },
                notifications.ToArray());
        }

        [TestMethod]
        public async Task ReplaySubject_WorkflowCompleted_ReplaysBufferToLateObserver()
        {
            // An observer subscribing after the scope ended receives the buffer followed by completion.
            var capture = new CaptureSequence<int>();
            var workflow = await BuildNestedWorkflow<int>(input => input
                .AppendOutput()
                .ResetCursor()
                .AppendSubject<ReplaySubject<int>>(nameof(ReplaySubject))
                .ResetCursor()
                .AppendSource(Observable.Range(1, 3, Scheduler.Immediate).Concat(Observable.Never<int>()))
                .Append(new MulticastSubject { Name = nameof(ReplaySubject) })
                .ResetCursor()
                .Append(new SubscribeSubject { Name = nameof(ReplaySubject) })
                .AppendCombinator(capture));

            await workflow;
            var notifications = await capture.Sequence.Materialize().ToList();
            CollectionAssert.AreEqual(
                new[]
                {
                    Notification.CreateOnNext(1),
                    Notification.CreateOnNext(2),
                    Notification.CreateOnNext(3),
                    Notification.CreateOnCompleted<int>()
                },
                notifications.ToArray());
        }

        [TestMethod]
        public async Task ResourceSubject_WorkflowCompleted_DisposesResource()
        {
            // The end of the scope disposes the stored resource and late observers receive it with completion.
            var resource = new BooleanDisposable();
            var capture = new CaptureSequence<BooleanDisposable>();
            var workflow = await BuildNestedWorkflow<int>(input => input
                .AppendOutput()
                .ResetCursor()
                .AppendSource(Observable.Return(resource))
                .Append(new ResourceSubject { Name = nameof(ResourceSubject) })
                .ResetCursor()
                .Append(new SubscribeSubject { Name = nameof(ResourceSubject) })
                .AppendCombinator(capture));

            await workflow;
            var notifications = await capture.Sequence.Materialize().ToList();
            Assert.IsTrue(resource.IsDisposed);
            CollectionAssert.AreEqual(
                new[] { Notification.CreateOnNext(resource), Notification.CreateOnCompleted<BooleanDisposable>() },
                notifications.ToArray());
        }

        [TestMethod]
        public async Task ResourceSubject_WorkflowCompletes_CompletesActiveObserver()
        {
            // The end of the scope completes the observers of a subject with no stored resource.
            var capture = new CaptureSequence<BooleanDisposable>();
            var workflow = await BuildNestedWorkflow<int>(input => input
                .AppendOutput()
                .ResetCursor()
                .AppendSubject<ResourceSubject<BooleanDisposable>>(nameof(ResourceSubject))
                .ResetCursor()
                .Append(new SubscribeSubject { Name = nameof(ResourceSubject) })
                .AppendCombinator(capture));

            var notifications = Observe(capture.Sequence);
            await workflow;
            CollectionAssert.AreEqual(new[] { Notification.CreateOnCompleted<BooleanDisposable>() }, notifications);
        }

        [TestMethod]
        public async Task ResourceSubject_WriterIncomplete_DisposesResource()
        {
            // A resource from a writer that never completes reaches active observers and is disposed when the scope ends.
            var resource = new BooleanDisposable();
            var capture = new CaptureSequence<BooleanDisposable>();
            var workflow = await BuildNestedWorkflow<int>(input => input
                .AppendOutput()
                .ResetCursor()
                .AppendSource(Observable.Return(resource).Concat(Observable.Never<BooleanDisposable>()))
                .Append(new ResourceSubject { Name = nameof(ResourceSubject) })
                .ResetCursor()
                .Append(new SubscribeSubject { Name = nameof(ResourceSubject) })
                .AppendCombinator(capture));

            var notifications = Observe(capture.Sequence);
            await workflow;
            Assert.IsTrue(resource.IsDisposed);
            CollectionAssert.AreEqual(
                new[] { Notification.CreateOnNext(resource), Notification.CreateOnCompleted<BooleanDisposable>() },
                notifications);
        }

        [TestMethod]
        public async Task ResourceSubject_SecondSubscription_DisposesResourceOnce()
        {
            // A workflow sequence subscribed again after the scope ended does not dispose the stored resource again.
            var resource = new CountingDisposable();
            var workflow = new TestWorkflow()
                .AppendSource(Observable.Return(0))
                .AppendOutput()
                .ResetCursor()
                .AppendSource(Observable.Return(resource))
                .Append(new ResourceSubject { Name = nameof(ResourceSubject) })
                .BuildObservable<int>();

            await workflow;
            await workflow;
            Assert.AreEqual(1, resource.DisposeCount);
        }

        static IObservable<IObservable<T>> BuildNestedWorkflow<T>(Func<TestWorkflow, TestWorkflow> selector)
        {
            return new TestWorkflow()
                .AppendSource(Observable.Return(0))
                .AppendNested(selector, workflow => new CreateObservable(workflow))
                .AppendOutput()
                .BuildObservable<IObservable<T>>();
        }

        static IObservable<IObservable<int>> BuildBehaviorSubjectWorkflow(CaptureSequence<int> capture)
        {
            return BuildNestedWorkflow<int>(input => input
                .AppendOutput()
                .ResetCursor()
                .AppendSource(Observable.Return(1))
                .Append(new BehaviorSubject { Name = nameof(BehaviorSubject) })
                .ResetCursor()
                .AppendSource(Observable.Return(2))
                .Append(new MulticastSubject { Name = nameof(BehaviorSubject) })
                .ResetCursor()
                .Append(new SubscribeSubject { Name = nameof(BehaviorSubject) })
                .AppendCombinator(capture));
        }

        static List<Notification<T>> Observe<T>(IObservable<T> source)
        {
            var notifications = new List<Notification<T>>();
            source.Materialize().Subscribe(notifications.Add);
            return notifications;
        }

        [Combinator]
        class CaptureSequence<T> : Combinator<T, T>
        {
            public IObservable<T> Sequence { get; private set; }

            public override IObservable<T> Process(IObservable<T> source)
            {
                Sequence = source;
                return Observable.Empty<T>();
            }
        }

        class CountingDisposable : IDisposable
        {
            public int DisposeCount { get; private set; }

            public void Dispose()
            {
                DisposeCount++;
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
