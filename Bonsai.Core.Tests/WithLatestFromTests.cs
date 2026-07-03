using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Bonsai.Reactive;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class WithLatestFromTests
    {
        static IObservable<Tuple<TSource, TOther>> Process<TSource, TOther>(
            IObservable<TSource> source,
            IObservable<TOther> other)
        {
            return new WithLatestFrom().Process(source, other);
        }

        [TestMethod]
        public void Process_SourceEmitsAfterValueSeen_EmitsPairWithLatestValue()
        {
            var source = new Subject<int>();
            var other = new Subject<string>();
            var results = new List<Tuple<int, string>>();
            using (Process(source, other).Subscribe(results.Add))
            {
                other.OnNext("a");
                source.OnNext(1);
                other.OnNext("b");
                source.OnNext(2);
            }

            CollectionAssert.AreEqual(
                new[] { Tuple.Create(1, "a"), Tuple.Create(2, "b") },
                results);
        }

        [TestMethod]
        public void Process_SourceCompletes_CompletesResult()
        {
            var source = new Subject<int>();
            var other = new Subject<string>();
            var completed = false;
            using (Process(source, other).Subscribe(_ => { }, () => completed = true))
            {
                other.OnNext("a");
                source.OnCompleted();
            }

            Assert.IsTrue(completed);
        }

        [TestMethod]
        public void Process_ValueSequenceCompletes_ContinuesWithLastValue()
        {
            var source = new Subject<int>();
            var other = new Subject<string>();
            var results = new List<Tuple<int, string>>();
            var completed = false;
            using (Process(source, other).Subscribe(results.Add, () => completed = true))
            {
                other.OnNext("a");
                other.OnCompleted();
                source.OnNext(1);
                source.OnNext(2);
            }

            Assert.IsFalse(completed);
            CollectionAssert.AreEqual(
                new[] { Tuple.Create(1, "a"), Tuple.Create(2, "a") },
                results);
        }

        [TestMethod]
        public void Process_SourceEmitsBeforeAnyValue_DropsNotification()
        {
            var source = new Subject<int>();
            var other = new Subject<string>();
            var results = new List<Tuple<int, string>>();
            using (Process(source, other).Subscribe(results.Add))
            {
                source.OnNext(1);
                other.OnNext("a");
                source.OnNext(2);
            }

            CollectionAssert.AreEqual(
                new[] { Tuple.Create(2, "a") },
                results);
        }

        [TestMethod]
        public void Process_SourceErrors_FaultsDownstream()
        {
            var source = new Subject<int>();
            var other = new Subject<string>();
            var error = new InvalidOperationException();
            Exception observed = null;
            using (Process(source, other).Subscribe(_ => { }, ex => observed = ex))
            {
                other.OnNext("a");
                source.OnError(error);
            }

            Assert.AreSame(error, observed);
        }

        [TestMethod]
        public void Process_ValueSequenceErrors_FaultsDownstream()
        {
            var source = new Subject<int>();
            var other = new Subject<string>();
            var error = new InvalidOperationException();
            Exception observed = null;
            using (Process(source, other).Subscribe(_ => { }, ex => observed = ex))
            {
                other.OnError(error);
            }

            Assert.AreSame(error, observed);
        }

        [TestMethod]
        public void Process_ValueAvailableSynchronously_EmitsOnImmediateSourceElement()
        {
            var source = Observable.Return(1);
            var other = Observable.Return("a");
            var results = new List<Tuple<int, string>>(Process(source, other).ToList().Wait());

            CollectionAssert.AreEqual(
                new[] { Tuple.Create(1, "a") },
                results);
        }
    }
}
