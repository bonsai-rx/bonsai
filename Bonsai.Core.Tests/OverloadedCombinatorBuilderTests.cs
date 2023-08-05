using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Reactive;

namespace Bonsai.Core.Tests
{
    public partial class CombinatorBuilderTests
    {
        [Combinator]
        class OverloadedCombinatorMock
        {
            public IObservable<float> Process(IObservable<float> source) => source;
            public IObservable<double> Process(IObservable<double> source) => source;
        }

        [Combinator]
        class ParamsOverloadedCombinatorMock
        {
            public IObservable<float> Process(params IObservable<float>[] source)
            {
                return source.FirstOrDefault();
            }

            public IObservable<double> Process(params IObservable<double>[] source)
            {
                return source.FirstOrDefault();
            }
        }

        [Combinator]
        class GenericOverloadedCombinatorMock
        {
            public IObservable<float> Process(IObservable<float> _) => Observable.Return(float.NaN);

            public IObservable<TSource> Process<TSource>(IObservable<TSource> source) => source;
        }

        [Combinator]
        class ListTupleOverloadedCombinatorMock
        {
            public IObservable<int> Process(IObservable<Tuple<int, int>> source)
                => source.Select(x => x.Item1);
            public IObservable<IList<int>> Process(IObservable<IList<int>> source) => source;
        }

        [Combinator]
        class AmbiguousOverloadedCombinatorMock
        {
            public IObservable<int> Process(IObservable<int> source1, IObservable<double> source2)
            {
                return source1;
            }

            public IObservable<int> Process(IObservable<double> source1, IObservable<int> source2)
            {
                return source2;
            }

            public IObservable<int> Process(IObservable<object> source1, IObservable<object> source2)
            {
                return null;
            }
        }

        [Combinator]
        class SpecializedGenericOverloadedCombinatorMock
        {
            public IObservable<TSource> Process<TSource>(IObservable<TSource> source) => source;
            public IObservable<TSource> Process<TSource>(IObservable<Timestamped<TSource>> source)
                => source.Select(x => x.Value);
        }

        class HidingOverloadedCombinatorMock : OverloadedCombinatorMock
        {
            public new IObservable<double> Process(IObservable<double> _) => Observable.Return(double.NaN);
        }

        class HidingSpecializedGenericOverloadedCombinatorMock : SpecializedGenericOverloadedCombinatorMock
        {
            public new IObservable<TSource> Process<TSource>(IObservable<Timestamped<TSource>> source)
            {
                return source.Select(x => default(TSource));
            }
        }

        [Combinator]
        class BaseVirtualCombinatorMock
        {
            public virtual IObservable<string> Process(IObservable<string> source) => source;
        }

        class DerivedOverrideCombinatorMock : BaseVirtualCombinatorMock
        {
            public override IObservable<string> Process(IObservable<string> source)
                => Observable.Return(string.Empty);
        }

        class DerivedOverrideOverloadedCombinatorMock : BaseVirtualCombinatorMock
        {
            public override IObservable<string> Process(IObservable<string> source) => source;

            public IObservable<object> Process(IObservable<object> _) =>
                Observable.Return(default(object));
        }

        [Combinator]
        abstract class BaseGenericOverloadedCombinatorMock
        {
            public abstract IObservable<TSource> Process<TSource>(IObservable<TSource> source);
        }

        class DerivedOverrideGenericOverloadedCombinatorMock : BaseGenericOverloadedCombinatorMock
        {
            public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
                => Observable.Empty<TSource>();

            public IObservable<EventArgs> Process(IObservable<EventArgs> source) => source;
        }

        class DerivedOverridePrimitiveTransformMock : Transform<double, double>
        {
            public override IObservable<double> Process(IObservable<double> source) => source;
            public IObservable<decimal> Process(IObservable<decimal> source) => source;
        }

        private TResult RunOverload<TSource, TResult, TCombinator>(TSource value) where TCombinator : new()
        {
            return RunOverload<TSource, TResult, TCombinator>(Observable.Return(value));
        }

        private TResult RunOverload<TSource, TResult, TCombinator>(IObservable<TSource> value) where TCombinator : new()
        {
            var combinator = new TCombinator();
            var source = CreateObservableExpression(value);
            var resultProvider = TestCombinatorBuilder<TResult>(combinator, source);
            return Last(resultProvider).Result;
        }

        private void AssertOverloadEquals<TSource, TCombinator>(TSource value) where TCombinator : new()
        {
            var result = RunOverload<TSource, TSource, TCombinator>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadDoubleMethodCalledWithDouble_ReturnsDoubleValue()
        {
            AssertOverloadEquals<double, OverloadedCombinatorMock>(5.0);
        }

        [TestMethod]
        public void Build_OverloadFloatMethodCalledWithInt_ReturnsFloatValue()
        {
            var value = 5;
            var result = RunOverload<int, float, OverloadedCombinatorMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadParamsFloatMethodCalledWithInt_ReturnsFloatValue()
        {
            var value = 5;
            var result = RunOverload<int, float, ParamsOverloadedCombinatorMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadGenericFloatMethodCalledWithFloat_ReturnsSpecializedResult()
        {
            var value = 5.0f;
            var result = RunOverload<float, float, GenericOverloadedCombinatorMock>(value);
            Assert.AreEqual(float.NaN, result);
        }

        [TestMethod]
        public void Build_OverloadListTupleMethodCalledWithIntTuple_ReturnsIntValue()
        {
            var value = Tuple.Create(5, 1);
            var result = RunOverload<Tuple<int, int>, int, ListTupleOverloadedCombinatorMock>(value);
            Assert.AreEqual(value.Item1, result);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_OverloadAmbiguousMethodCalledWithIntTuple_ThrowsWorkflowBuildException()
        {
            var value = 5;
            var source1 = CreateObservableExpression(Observable.Return(value));
            var source2 = CreateObservableExpression(Observable.Return(value));
            var resultProvider = TestCombinatorBuilder<int, AmbiguousOverloadedCombinatorMock>(source1, source2);
            var result = Last(resultProvider).Result;
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadAverageMethodCalledWithLong_ReturnsDoubleValue()
        {
            var value = 5L;
            var result = RunOverload<long, double, Reactive.Average>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadSpecializedGenericMethod_ReturnsValue()
        {
            var value = 5;
            var result = RunOverload<Timestamped<int>, int, SpecializedGenericOverloadedCombinatorMock>(
                Observable.Return(value).Timestamp());
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadHidingDoubleMethodCalledWithDouble_ReturnsDoubleValue()
        {
            var value = 5.0;
            var result = RunOverload<double, double, HidingOverloadedCombinatorMock>(value);
            Assert.AreNotEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadHidingSpecializedGenericMethod_ReturnsValue()
        {
            var value = 5;
            var result = RunOverload<Timestamped<int>, int, HidingSpecializedGenericOverloadedCombinatorMock>(
                Observable.Return(value).Timestamp());
            Assert.AreNotEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadDerivedOverrideMethodCalledWithString_ReturnsOverrideValue()
        {
            var value = "5";
            var result = RunOverload<string, string, DerivedOverrideCombinatorMock>(value);
            Assert.AreNotEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadDerivedOverrideMethodCalledWithString_ReturnsObjectValue()
        {
            var value = "5";
            var result = RunOverload<string, object, DerivedOverrideOverloadedCombinatorMock>(value);
            Assert.AreNotEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadOverrideWithRefTypeAndCallWithObject_ReturnsObjectValue()
        {
            var value = new object();
            var result = RunOverload<object, object, DerivedOverrideGenericOverloadedCombinatorMock>(value);
            Assert.AreNotEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadOverrideCalledWithConvertibleValue_ReturnsOriginalTypeValue()
        {
            var value = 5.0;
            var result = RunOverload<double, double, DerivedOverridePrimitiveTransformMock>(value);
            Assert.AreNotEqual(value, result);
        }
    }
}
