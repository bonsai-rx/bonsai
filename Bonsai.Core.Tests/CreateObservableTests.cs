using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bonsai.Expressions;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rx = System.Reactive.Subjects;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class CreateObservableTests
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
        public async Task Build_InputSequenceSubscribedOnce_ReturnsSubjectValues()
        {
            var workflow = new TestWorkflow()
                .AppendCombinator(new Reactive.Range { Count = 2 })
                .AppendNested(
                    input => input
                        .Append(new MulticastSubject { Name = nameof(BehaviorSubject) })
                        .ResetCursor()
                        .Append(new SubscribeSubject { Name = nameof(BehaviorSubject) })
                        .AppendCombinator(new Take { Count = 1 })
                        .AppendOutput()
                        .ResetCursor()
                        .Append(new BehaviorSubject<int> { Name = nameof(BehaviorSubject) }),
                    workflow => new CreateObservable(workflow))
                .AppendNested(
                    input => input.AppendOutput(),
                    workflow => new SelectMany(workflow))
                .AppendOutput();
            var values = await workflow.BuildObservable<int>().ToArray();
            CollectionAssert.AreEqual(new[] { 0, 1 }, values);
        }

        [TestMethod]
        public async Task Build_InputSequenceSubscribedTwice_ReturnsSameValues()
        {
            // Each subscription to an emitted sequence creates its own local scope subjects
            var workflow = new TestWorkflow()
                .AppendCombinator(new Reactive.Range { Count = 2 })
                .AppendNested(
                    input => input
                        .Append(new MulticastSubject { Name = nameof(BehaviorSubject) })
                        .ResetCursor()
                        .Append(new SubscribeSubject { Name = nameof(BehaviorSubject) })
                        .AppendCombinator(new Take { Count = 1 })
                        .AppendOutput()
                        .ResetCursor()
                        .Append(new BehaviorSubject<int> { Name = nameof(BehaviorSubject) }),
                    workflow => new CreateObservable(workflow))
                .AppendNested(
                    input => input
                        .AppendCombinator(new RepeatCount { Count = 2 })
                        .AppendOutput(),
                    workflow => new SelectMany(workflow))
                .AppendOutput();
            var values = await workflow.BuildObservable<int>().ToArray();
            CollectionAssert.AreEqual(new[] { 0, 0, 1, 1 }, values);
        }

        [TestMethod]
        public async Task Build_InputSequenceOverlappingSubscriptions_DoNotShareSubjects()
        {
            var value = 42;
            var workflow = new TestWorkflow()
                .AppendValue(value)
                .AppendCombinator(new Take { Count = 1 })
                .AppendNested(
                    input => input
                        .Append(new MulticastSubject { Name = nameof(BehaviorSubject) })
                        .ResetCursor()
                        .Append(new SubscribeSubject { Name = nameof(BehaviorSubject) })
                        .AppendOutput()
                        .ResetCursor()
                        .Append(new BehaviorSubject<int> { Name = nameof(BehaviorSubject) }),
                    workflow => new CreateObservable(workflow))
                .AppendOutput();
            var sequence = await workflow.BuildObservable<IObservable<int>>();
            var first = new List<int>();
            var second = new List<int>();
            using (sequence.Subscribe(first.Add))
            using (sequence.Subscribe(second.Add))
            {
                CollectionAssert.AreEqual(new[] { value }, first);
                CollectionAssert.AreEqual(new[] { value }, second);
            }
        }

        [TestMethod]
        public async Task Build_NoInputSequenceSubscribedTwice_ReturnsSameValues()
        {
            var value = 42;
            var workflow = new TestWorkflow()
                .AppendNested(
                    source => source
                        .AppendValue(value)
                        .Append(new MulticastSubject { Name = nameof(BehaviorSubject) })
                        .ResetCursor()
                        .Append(new SubscribeSubject { Name = nameof(BehaviorSubject) })
                        .AppendCombinator(new Take { Count = 1 })
                        .AppendOutput()
                        .ResetCursor()
                        .Append(new BehaviorSubject<int> { Name = nameof(BehaviorSubject) }),
                    workflow => new CreateObservable(workflow))
                .AppendNested(
                    input => input
                        .AppendCombinator(new RepeatCount { Count = 2 })
                        .AppendOutput(),
                    workflow => new SelectMany(workflow))
                .AppendOutput();
            var values = await workflow.BuildObservable<int>().ToArray();
            CollectionAssert.AreEqual(new[] { value, value }, values);
        }

        [TestMethod]
        public void Build_InputMappingWithConcat_UsesPropertyValueAtArrival()
        {
            // Queued sequences keep the property values mapped when their notification arrived
            var counts = new[] { 2, 3, 1 };
            var items = new Rx.Subject<int>();
            var ticks = new Rx.Subject<int>();
            var workflow = new TestWorkflow()
                .Append(new ConstantExpressionBuilder { Expression = Expression.Constant(items, typeof(IObservable<int>)) })
                .Append(new InputMappingBuilder { PropertyMappings = { new PropertyMapping { Name = nameof(Take.Count) } } })
                .AppendNested(
                    input => input
                        .ResetCursor()
                        .Append(new ExternalizedMappingBuilder { ExternalizedProperties = { new ExternalizedMapping { Name = nameof(Take.Count) } } })
                        .AppendBranch(mapping => mapping
                            .ResetCursor()
                            .Append(new ConstantExpressionBuilder { Expression = Expression.Constant(ticks, typeof(IObservable<int>)) })
                            .AppendCombinator(new Take())
                            .AddArguments(mapping))
                        .AppendCombinator(new Reactive.Count())
                        .AppendOutput(),
                    workflow => new CreateObservable(workflow))
                .AppendCombinator(new Concat())
                .AppendOutput();
            var values = new List<int>();
            using (workflow.BuildObservable<int>().Subscribe(values.Add))
            {
                foreach (var count in counts)
                {
                    items.OnNext(count);
                }

                for (int i = 0; i < 10; i++)
                {
                    ticks.OnNext(i);
                }
            }

            CollectionAssert.AreEqual(counts, values);
        }

        [TestMethod]
        public void Build_InputSequenceResubscribed_UsesCurrentPropertyValue()
        {
            // Subscriptions after the first evaluate the nested workflow again
            var ticks = new Rx.Subject<int>();
            var take = new Take { Count = 2 };
            var workflow = new TestWorkflow()
                .AppendValue(0)
                .AppendCombinator(new Take { Count = 1 })
                .AppendNested(
                    input => input
                        .ResetCursor()
                        .Append(new ConstantExpressionBuilder { Expression = Expression.Constant(ticks, typeof(IObservable<int>)) })
                        .AppendCombinator(take)
                        .AppendCombinator(new Reactive.Count())
                        .AppendOutput(),
                    workflow => new CreateObservable(workflow))
                .AppendNested(
                    input => input
                        .AppendCombinator(new RepeatCount { Count = 2 })
                        .AppendOutput(),
                    workflow => new SelectMany(workflow))
                .AppendOutput();
            var values = new List<int>();
            using (workflow.BuildObservable<int>().Subscribe(values.Add))
            {
                take.Count = 4;
                for (int i = 0; i < 10; i++)
                {
                    ticks.OnNext(i);
                }
            }

            CollectionAssert.AreEqual(new[] { 2, 4 }, values);
        }

        [TestMethod]
        public void Build_NoInputPropertyMappingWithConcat_UsesPropertyValueAtArrival()
        {
            // Queued sequences keep the property values mapped before their nested workflow was evaluated
            var counts = new[] { 2, 3, 1 };
            var items = new Rx.Subject<int>();
            var ticks = new Rx.Subject<int>();
            var workflow = new TestWorkflow()
                .Append(new ConstantExpressionBuilder { Expression = Expression.Constant(items, typeof(IObservable<int>)) })
                .AppendNested(
                    input => input
                        .AppendPropertyMapping(nameof(Take.Count))
                        .AppendNested(
                            source => source
                                .ResetCursor()
                                .Append(new ExternalizedMappingBuilder { ExternalizedProperties = { new ExternalizedMapping { Name = nameof(Take.Count) } } })
                                .AppendBranch(mapping => mapping
                                    .ResetCursor()
                                    .Append(new ConstantExpressionBuilder { Expression = Expression.Constant(ticks, typeof(IObservable<int>)) })
                                    .AppendCombinator(new Take())
                                    .AddArguments(mapping))
                                .AppendCombinator(new Reactive.Count())
                                .AppendOutput(),
                            workflow => new CreateObservable(workflow))
                        .AppendOutput(),
                    workflow => new SelectMany(workflow))
                .AppendCombinator(new Concat())
                .AppendOutput();
            var values = new List<int>();
            using (workflow.BuildObservable<int>().Subscribe(values.Add))
            {
                foreach (var count in counts)
                {
                    items.OnNext(count);
                }

                for (int i = 0; i < 10; i++)
                {
                    ticks.OnNext(i);
                }
            }

            CollectionAssert.AreEqual(counts, values);
        }

        [TestMethod]
        public async Task Build_NoInputSequenceResubscribed_FirstUsesPropertyValueAtEvaluation()
        {
            // The first subscription uses the nested workflow evaluated with the operator, later ones evaluate it again
            var ticks = new Rx.Subject<int>();
            var take = new Take { Count = 2 };
            var workflow = new TestWorkflow()
                .AppendNested(
                    source => source
                        .Append(new ConstantExpressionBuilder { Expression = Expression.Constant(ticks, typeof(IObservable<int>)) })
                        .AppendCombinator(take)
                        .AppendCombinator(new Reactive.Count())
                        .AppendOutput(),
                    workflow => new CreateObservable(workflow))
                .AppendOutput();
            var sequence = await workflow.BuildObservable<IObservable<int>>();
            var values = new List<int>();
            foreach (var count in new[] { 3, 4 })
            {
                take.Count = count;
                using (sequence.Subscribe(values.Add))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        ticks.OnNext(i);
                    }
                }
            }

            CollectionAssert.AreEqual(new[] { 2, 4 }, values);
        }
    }
}
