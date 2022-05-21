using System;
using System.Collections.Generic;

namespace Bonsai
{
    /// <summary>
    /// Represents a range of values defined by an inclusive lower and upper bounds.
    /// </summary>
    /// <typeparam name="TValue">The type of values in the range.</typeparam>
    public sealed class Range<TValue> : IEquatable<Range<TValue>>
    {
        readonly IComparer<TValue> valueComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Range{TValue}"/> class with the specified
        /// lower and upper bounds.
        /// </summary>
        /// <param name="lowerBound">The inclusive lower bound of the range.</param>
        /// <param name="upperBound">The inclusive upper bound of the range.</param>
        public Range(TValue lowerBound, TValue upperBound)
            : this(lowerBound, upperBound, Comparer<TValue>.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Range{TValue}"/> class with the specified 
        /// lower and upper bounds and using the specified comparer.
        /// </summary>
        /// <param name="lowerBound">The inclusive lower bound of the range.</param>
        /// <param name="upperBound">The inclusive upper bound of the range.</param>
        /// <param name="comparer">An <see cref="IComparer{TValue}"/> to use to compare values.</param>
        public Range(TValue lowerBound, TValue upperBound, IComparer<TValue> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }

            if (comparer.Compare(lowerBound, upperBound) > 0)
            {
                throw new ArgumentException("Lower bound must be lower or equal to upper bound.", nameof(lowerBound));
            }

            LowerBound = lowerBound;
            UpperBound = upperBound;
            valueComparer = comparer;
        }

        /// <summary>
        /// Gets the inclusive lower bound of the range.
        /// </summary>
        public TValue LowerBound { get; private set; }

        /// <summary>
        /// Gets the inclusive upper bound of the range.
        /// </summary>
        public TValue UpperBound { get; private set; }

        /// <summary>
        /// Tests whether a specified value falls within the range.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="value"/> is between or equal to the
        /// lower and upper bound values of this instance; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Contains(TValue value)
        {
            return valueComparer.Compare(LowerBound, value) <= 0 &&
                valueComparer.Compare(value, UpperBound) <= 0;
        }

        /// <summary>
        /// Returns a value indicating whether this instance has the same lower and upper
        /// bounds as a specified <see cref="Range{TValue}"/> object.
        /// </summary>
        /// <param name="other">The <see cref="Range{TValue}"/> object to compare to this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="other"/> has the same lower and upper
        /// bounds as this instance; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Range<TValue> other)
        {
            return other != null &&
                EqualityComparer<TValue>.Default.Equals(LowerBound, other.LowerBound) &&
                EqualityComparer<TValue>.Default.Equals(UpperBound, other.UpperBound);
        }

        /// <summary>
        /// Tests to see whether the specified object is a <see cref="Range{TValue}"/> object
        /// with the same lower and upper bounds as this <see cref="Range{TValue}"/> instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="Range{TValue}"/>
        /// and has the same lower and upper bounds as this <see cref="Range{TValue}"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Range<TValue> range)
            {
                return Equals(range);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>
        /// The hash code for the current instance.
        /// </returns>
        public override int GetHashCode()
        {
            return EqualityComparer<TValue>.Default.GetHashCode(LowerBound) + 31 *
                   EqualityComparer<TValue>.Default.GetHashCode(UpperBound);
        }

        /// <summary>
        /// Creates a <see cref="string"/> representation of this <see cref="Range{TValue}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing the lower and upper bound values of
        /// this <see cref="Range{TValue}"/>.
        /// </returns>
        public override string ToString()
        {
            return $"[{LowerBound}, {UpperBound}]";
        }

        /// <summary>
        /// Tests whether two <see cref="Range{TValue}"/> objects are equal.
        /// </summary>
        /// <param name="left">
        /// The <see cref="Range{TValue}"/> object on the left-hand side of the
        /// equality operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="Range{TValue}"/> object on the right-hand side of the
        /// equality operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// have equal lower and upper bounds; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Range<TValue> left, Range<TValue> right)
        {
            if (left is object) return left.Equals(right);
            else return right is null;
        }

        /// <summary>
        /// Tests whether two <see cref="Range{TValue}"/> objects are different.
        /// </summary>
        /// <param name="left">
        /// The <see cref="Range{TValue}"/> object on the left-hand side of the
        /// inequality operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="Range{TValue}"/> object on the right-hand side of the
        /// inequality operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// differ either in their lower or upper bounds; <see langword="false"/> if
        /// <paramref name="left"/> and <paramref name="right"/> are equal.
        /// </returns>
        public static bool operator !=(Range<TValue> left, Range<TValue> right)
        {
            if (left is object) return !left.Equals(right);
            else return right is object;
        }
    }

    /// <summary>
    /// Provides static methods for creating range objects.
    /// </summary>
    public static class Range
    {
        /// <summary>
        /// Creates a new range with the specified lower and upper bounds.
        /// </summary>
        /// <typeparam name="TValue">The type of values in the range.</typeparam>
        /// <param name="lowerBound">The inclusive lower bound of the range.</param>
        /// <param name="upperBound">The inclusive lower bound of the range.</param>
        /// <returns>A new instance of the <see cref="Range{TValue}"/> class.</returns>
        public static Range<TValue> Create<TValue>(TValue lowerBound, TValue upperBound)
        {
            return new Range<TValue>(lowerBound, upperBound);
        }

        /// <summary>
        /// Creates a new range with the specified lower and upper bounds and
        /// using the specified comparer.
        /// </summary>
        /// <typeparam name="TValue">The type of values in the range.</typeparam>
        /// <param name="lowerBound">The inclusive lower bound of the range.</param>
        /// <param name="upperBound">The inclusive lower bound of the range.</param>
        /// <param name="comparer">An <see cref="IComparer{TValue}"/> to use to compare values.</param>
        /// <returns>A new instance of the <see cref="Range{TValue}"/> class.</returns>
        public static Range<TValue> Create<TValue>(TValue lowerBound, TValue upperBound, IComparer<TValue> comparer)
        {
            return new Range<TValue>(lowerBound, upperBound, comparer);
        }
    }
}
