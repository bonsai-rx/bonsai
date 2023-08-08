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
            public IObservable<float> Process(params IObservable<float>[] source) => source.FirstOrDefault();
            public IObservable<double> Process(params IObservable<double>[] source) => source.FirstOrDefault();
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
            public IObservable<int> Process(IObservable<int> source1, IObservable<double> _) => source1;
            public IObservable<int> Process(IObservable<double> _, IObservable<int> source2) => source2;
            public IObservable<int> Process(IObservable<object> _, IObservable<object> __) => null;
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
                => source.Select(x => default(TSource));
        }

        [Combinator]
        abstract class BaseVirtualCombinatorMock
        {
            public abstract IObservable<string> Process(IObservable<string> source);
        }

        class DerivedOverrideCombinatorMock : BaseVirtualCombinatorMock
        {
            public override IObservable<string> Process(IObservable<string> source) => source;
        }

        class DerivedOverrideOverloadedCombinatorMock : BaseVirtualCombinatorMock
        {
            public override IObservable<string> Process(IObservable<string> source) => source;
            public IObservable<object> Process(IObservable<object> source) => source;
        }

        [Combinator]
        abstract class BaseGenericOverloadedCombinatorMock
        {
            public abstract IObservable<TSource> Process<TSource>(IObservable<TSource> source);
        }

        class DerivedOverrideGenericOverloadedCombinatorMock : BaseGenericOverloadedCombinatorMock
        {
            public override IObservable<TSource> Process<TSource>(IObservable<TSource> source) => source;
            public IObservable<EventArgs> Process(IObservable<EventArgs> source) => source;
        }

        class DerivedOverridePrimitiveTransformMock : Transform<float, float>
        {
            public override IObservable<float> Process(IObservable<float> source) => source;
            public IObservable<double> Process(IObservable<double> source) => source;
        }

        class DerivedOverrideCovariantTransformMock : Transform<object[], object[]>
        {
            public override IObservable<object[]> Process(IObservable<object[]> source) => source;
            public IObservable<Array> Process(IObservable<Array> source) => source;
        }

        [Combinator]
        class MultiArgumentBaseCovariantMock
        {
            public virtual IObservable<object[]> Process(
                IObservable<object[]> source,
                IObservable<object> source2) => source;
        }

        class MultiArgumentDerivedCovariantMock : MultiArgumentBaseCovariantMock
        {
            public override IObservable<object[]> Process(
                IObservable<object[]> source,
                IObservable<object> source2) => source;

            public IObservable<Array> Process(
                IObservable<Array> source,
                IObservable<string> _) => source;
        }

        private TResult RunOverload<TSource, TResult, TCombinator>(TSource value)
            where TCombinator : new()
        {
            return RunOverload<TSource, TResult, TCombinator>(Observable.Return(value));
        }

        private TResult RunOverload<TSource, TResult, TCombinator>(IObservable<TSource> value)
            where TCombinator : new()
        {
            var combinator = new TCombinator();
            var source = CreateObservableExpression(value);
            var resultProvider = TestCombinatorBuilder<TResult>(combinator, source);
            return Last(resultProvider).Result;
        }

        [TestMethod]
        public void Build_OverloadCallWithExactSignature_PreferExactMatch()
        // Same class overload exactly matching the argument signature is always preferred
        {
            var value = 5.0;
            var result = RunOverload<double, double, OverloadedCombinatorMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadMethodCalledWithImplicitConversion_PreferClosestConversion()
        // Same class overload with closest implicit conversion is preferred
        {
            var value = 5;
            var result = RunOverload<int, float, OverloadedCombinatorMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadParamsMethodWithImplicitConversion_PreferClosestConversion()
        // Same class params overload with closest implicit conversion is preferred
        {
            var value = 5;
            var result = RunOverload<int, float, ParamsOverloadedCombinatorMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadGenericMethodAndNonGenericMethod_PreferNonGenericMethod()
        // Non-generic method signature is preferred over matching generic overload
        {
            var value = 5.0f;
            var result = RunOverload<float, float, GenericOverloadedCombinatorMock>(value);
            Assert.AreEqual(float.NaN, result);
        }

        [TestMethod]
        public void Build_OverloadMethodWithDifferentReturnSignature_ReturnValueFromPreferredOverload()
        // Return type signature depends on preferred method overload
        {
            var value = Tuple.Create(5, 1);
            var result = RunOverload<Tuple<int, int>, int, ListTupleOverloadedCombinatorMock>(value);
            Assert.AreEqual(value.Item1, result);
        }

        [TestMethod]
        [ExpectedException(typeof(WorkflowBuildException))]
        public void Build_OverloadAmbiguousMethodCall_ThrowsWorkflowBuildException()
        // Ambiguous overloaded method call throws build exception
        {
            var value = 5;
            var source1 = CreateObservableExpression(Observable.Return(value));
            var source2 = CreateObservableExpression(Observable.Return(value));
            var resultProvider = TestCombinatorBuilder<int, AmbiguousOverloadedCombinatorMock>(source1, source2);
            var result = Last(resultProvider).Result;
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_NullableOverloadMethodCalledWithImplicitConversion_PreferNonNullableClosestConversion()
        // Same class non-nullable overload with closest implicit conversion is preferred
        {
            var value = 5L;
            var result = RunOverload<long, double, Reactive.Average>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadSpecializedGenericMethod_PreferSpecializedOverload()
        // More specialized generic method signature is preferred over more general one
        {
            var value = 5;
            var result = RunOverload<Timestamped<int>, int, SpecializedGenericOverloadedCombinatorMock>(
                Observable.Return(value).Timestamp());
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadHidingBaseMethod_PreferNewOverload()
        // New overload with exact method signature hides base class implementation
        {
            var value = 5.0;
            var result = RunOverload<double, double, HidingOverloadedCombinatorMock>(value);
            Assert.AreNotEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadHidingSpecializedGenericMethod_PreferNewOverload()
        // New overload with exact method signature hides base class implementation
        {
            var value = 5;
            var result = RunOverload<Timestamped<int>, int, HidingSpecializedGenericOverloadedCombinatorMock>(
                Observable.Return(value).Timestamp());
            Assert.AreNotEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadOverrideCalledWithExactType_ReturnOverrideValue()
        // Override overload from abstract base class calls derived implementation
        {
            var value = "5";
            var result = RunOverload<string, string, DerivedOverrideCombinatorMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadOverrideAndNewOverloadWithBaseType_PreferNewOverload()
        // New overload excludes base class method since argument matches covariant signature without conversion
        {
            var value = "5";
            var result = RunOverload<string, object, DerivedOverrideOverloadedCombinatorMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadGenericOverrideAndNewOverloadWithUnrelatedType_PreferGenericExactMatch()
        // Base overload is preferred since no type conversion is required to match a generic method
        {
            var value = new object();
            var result = RunOverload<object, object, DerivedOverrideGenericOverloadedCombinatorMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadOverrideExactValueTypeAndNewOverloadWithImplicitConversion_PreferExactType()
        // Override is preferred since generic covariance does not apply to value types
        {
            var value = 5.0f;
            var result = RunOverload<float, float, DerivedOverridePrimitiveTransformMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadOverrideValueTypeWithImplicitConversion_PreferClosestConversionOverride()
        // Override is preferred since generic covariance does not apply to value types
        {
            var value = 5;
            var result = RunOverload<int, float, DerivedOverridePrimitiveTransformMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadOverrideAndNewOverloadWithBaseTypeAfterExplicitConversion_PreferNewOverload()
        // New overload is preferred since conversion to base override signature would match new covariant signature
        {
            var value = new object[1];
            var result = RunOverload<object, Array, DerivedOverrideCovariantTransformMock>(value);
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadOverrideAndNewMultiArgumentWithMixedConversions_PreferBaseOverload()
        // Override is preferred since method signature is not subsumed and second argument is exact match
        {
            var value = new object[1];
            var source1 = CreateObservableExpression(Observable.Return((object)value));
            var source2 = CreateObservableExpression(Observable.Return((object)value));
            var resultProvider = TestCombinatorBuilder<object[], MultiArgumentDerivedCovariantMock>(source1, source2);
            var result = Last(resultProvider).Result;
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        public void Build_OverloadOverrideAndNewMultiArgumentWithSpecializedConversion_PreferNewOverload()
        // New overload is preferred since the second argument is more specialized
        {
            var value1 = new object[1];
            var value2 = string.Empty;
            var source1 = CreateObservableExpression(Observable.Return((object)value1));
            var source2 = CreateObservableExpression(Observable.Return(value2));
            var resultProvider = TestCombinatorBuilder<Array, MultiArgumentDerivedCovariantMock>(source1, source2);
            var result = Last(resultProvider).Result;
            Assert.AreEqual(value1, result);
        }
    }
}
