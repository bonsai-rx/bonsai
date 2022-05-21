using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Core.Tests
{
    static class EquatableTests
    {
        public static void AssertEquatable<T>(T left, T right, bool expected) where T : IEquatable<T>
        {
            Assert.IsNotNull(left);
            Assert.IsNotNull(right);
            var genericEquals = left.Equals(right);
            var nonGenericEquals = left.Equals((object)right);
            var leftHashCode = left.GetHashCode();
            var rightHashCode = right.GetHashCode();
            var operatorEquals = EquatableOperators<T>.Equal(left, right);
            var operatorNotEquals = EquatableOperators<T>.NotEqual(left, right);
            Assert.AreEqual(expected, genericEquals);
            Assert.AreEqual(genericEquals, nonGenericEquals);
            Assert.AreEqual(genericEquals, operatorEquals);
            Assert.AreEqual(genericEquals, !operatorNotEquals);
            if (genericEquals) Assert.AreEqual(leftHashCode, rightHashCode);
        }
    }

    internal static class EquatableOperators<T> where T : IEquatable<T>
    {
        public static readonly Func<T, T, bool> Equal = GetBinaryOperator<T, bool>((left, right) => Expression.Equal(left, right));
        public static readonly Func<T, T, bool> NotEqual = GetBinaryOperator<T, bool>((left, right) => Expression.NotEqual(left, right));

        static Func<TValue, TValue, TResult> GetBinaryOperator<TValue, TResult>(Func<ParameterExpression, ParameterExpression, BinaryExpression> bodyConstructor)
        {
            var left = Expression.Parameter(typeof(TValue));
            var right = Expression.Parameter(typeof(TValue));
            return Expression.Lambda<Func<TValue, TValue, TResult>>(
                bodyConstructor(left, right),
                left, right)
                .Compile();
        }
    }
}
