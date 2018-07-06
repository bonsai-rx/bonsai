using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Bonsai.Expressions;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public class ExpressionBuilderArgumentTests
    {
        [TestMethod]
        public void Equals_ZeroZero_ReturnsTrue()
        {
            var argument = new ExpressionBuilderArgument();
            var zero = new ExpressionBuilderArgument();
            Assert.AreEqual(true, argument.Equals(zero));
        }

        [TestMethod]
        public void CompareTo_Null_ReturnsGreaterThanZero()
        {
            var argument = new ExpressionBuilderArgument();
            Assert.IsTrue(argument.CompareTo(null) > 0);
        }

        [TestMethod]
        public void CompareTo_ZeroZero_ReturnsZero()
        {
            var argument = new ExpressionBuilderArgument();
            var zero = new ExpressionBuilderArgument();
            Assert.AreEqual(0, argument.CompareTo(zero));
        }

        [TestMethod]
        public void CompareTo_ZeroOne_ReturnsLessThanZero()
        {
            var argument = new ExpressionBuilderArgument();
            var one = new ExpressionBuilderArgument(1);
            Assert.IsTrue(argument.CompareTo(one) < 0);
        }

        [TestMethod]
        public void CompareTo_OneZero_ReturnsGreaterThanZero()
        {
            var argument = new ExpressionBuilderArgument(1);
            var zero = new ExpressionBuilderArgument();
            Assert.IsTrue(argument.CompareTo(zero) > 0);
        }

        [TestMethod]
        public void CompareToNonGeneric_Null_ReturnsGreaterThanZero()
        {
            IComparable argument = new ExpressionBuilderArgument();
            Assert.AreEqual(1, argument.CompareTo(null));
        }

        [TestMethod]
        public void LessThanOperator_NullNull_ReturnsFalse()
        {
            var left = default(ExpressionBuilderArgument);
            var right = default(ExpressionBuilderArgument);
            Assert.IsFalse(left < right);
        }

        [TestMethod]
        public void LessThanOperator_NullZero_ReturnsTrue()
        {
            var left = default(ExpressionBuilderArgument);
            var right = new ExpressionBuilderArgument();
            Assert.IsTrue(left < right);
        }

        [TestMethod]
        public void LessThanOperator_ZeroZero_ReturnsFalse()
        {
            var left = new ExpressionBuilderArgument();
            var right = new ExpressionBuilderArgument();
            Assert.IsFalse(left < right);
        }

        [TestMethod]
        public void GreaterThanOperator_NullNull_ReturnsFalse()
        {
            var left = default(ExpressionBuilderArgument);
            var right = default(ExpressionBuilderArgument);
            Assert.IsFalse(left > right);
        }

        [TestMethod]
        public void GreaterThanOperator_NullZero_ReturnsFalse()
        {
            var left = default(ExpressionBuilderArgument);
            var right = new ExpressionBuilderArgument();
            Assert.IsFalse(left > right);
        }

        [TestMethod]
        public void GreaterThanOperator_ZeroZero_ReturnsFalse()
        {
            var left = new ExpressionBuilderArgument();
            var right = new ExpressionBuilderArgument();
            Assert.IsFalse(left > right);
        }
    }
}
