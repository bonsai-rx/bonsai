using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using Bonsai.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class BinaryOperatorBuilderTests
    {
        static bool EvaluateEquality<TSource>(ExpressionBuilder builder, TSource value)
        {
            var source = Expression.Constant(Observable.Return(value), typeof(IObservable<TSource>));
            var buildResult = builder.Build(new[] { source });
            var lambda = Expression.Lambda<Func<IObservable<bool>>>(buildResult);
            return lambda.Compile()().Wait();
        }

        [TestMethod]
        public void Equal_StructurallyEqualTuples_ReturnsTrue()
        {
            var value = Tuple.Create(Tuple.Create(4, 4), Tuple.Create(4, 4));
            Assert.IsTrue(EvaluateEquality(new EqualBuilder(), value));
        }

        [TestMethod]
        public void Equal_StructurallyUnequalTuples_ReturnsFalse()
        {
            var value = Tuple.Create(Tuple.Create(4, 4), Tuple.Create(4, 5));
            Assert.IsFalse(EvaluateEquality(new EqualBuilder(), value));
        }

        [TestMethod]
        public void NotEqual_StructurallyEqualTuples_ReturnsFalse()
        {
            var value = Tuple.Create(Tuple.Create(4, 4), Tuple.Create(4, 4));
            Assert.IsFalse(EvaluateEquality(new NotEqualBuilder(), value));
        }

        [TestMethod]
        public void NotEqual_StructurallyUnequalTuples_ReturnsTrue()
        {
            var value = Tuple.Create(Tuple.Create(4, 4), Tuple.Create(4, 5));
            Assert.IsTrue(EvaluateEquality(new NotEqualBuilder(), value));
        }

        [TestMethod]
        public void Equal_EqualPrimitivePair_ReturnsTrue()
        {
            var value = Tuple.Create(4, 4);
            Assert.IsTrue(EvaluateEquality(new EqualBuilder(), value));
        }

        [TestMethod]
        public void NotEqual_EqualPrimitivePair_ReturnsFalse()
        {
            var value = Tuple.Create(4, 4);
            Assert.IsFalse(EvaluateEquality(new NotEqualBuilder(), value));
        }

        [TestMethod]
        public void Equal_NestedStructurallyEqualTuples_ReturnsTrue()
        {
            var value = Tuple.Create(
                Tuple.Create(1, Tuple.Create(2, 3)),
                Tuple.Create(1, Tuple.Create(2, 3)));
            Assert.IsTrue(EvaluateEquality(new EqualBuilder(), value));
        }

        [TestMethod]
        public void Equal_StructurallyEqualValueTuples_ReturnsTrue()
        {
            var value = Tuple.Create((1, 2), (1, 2));
            Assert.IsTrue(EvaluateEquality(new EqualBuilder(), value));
        }

        [TestMethod]
        public void Equal_TupleContainingReferenceArrays_ComparesArraysByReference()
        {
            // Tuple equality compares elements with ==, so array elements compare by reference and equal but distinct arrays are unequal.
            var value = Tuple.Create(Tuple.Create(4, new[] { 1, 2, 3 }), Tuple.Create(4, new[] { 1, 2, 3 }));
            Assert.IsFalse(EvaluateEquality(new EqualBuilder(), value));
        }

        [TestMethod]
        public void Equal_TupleContainingNaN_ComparesNaNAsUnequal()
        {
            // Tuple equality compares elements with ==, so NaN is unequal, unlike Tuple.Equals which treats it as equal.
            var value = Tuple.Create(Tuple.Create(double.NaN, 1.0), Tuple.Create(double.NaN, 1.0));
            Assert.IsFalse(EvaluateEquality(new EqualBuilder(), value));
        }
    }
}
