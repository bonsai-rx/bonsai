using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Represents a range of values defined by an inclusive lower and upper bounds.
    /// </summary>
    /// <typeparam name="TValue">The type of values in the range.</typeparam>
    public sealed class Range<TValue>
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
                throw new ArgumentNullException("comparer");
            }

            if (comparer.Compare(lowerBound, upperBound) > 0)
            {
                throw new ArgumentException("Lower bound must be lower or equal to upper bound.", "lowerBound");
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
        /// <b>true</b> if <paramref name="value"/> is between or equal to <see cref="LowerBound"/>
        /// and <see cref="UpperBound"/>; <b>false</b> otherwise.
        /// </returns>
        public bool Contains(TValue value)
        {
            return valueComparer.Compare(LowerBound, value) <= 0 &&
                valueComparer.Compare(value, UpperBound) <= 0;
        }

        /// <summary>
        /// Creates a <see cref="String"/> representation of this <see cref="Range{TValue}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> containing the <see cref="LowerBound"/> and
        /// <see cref="UpperBound"/> values of this <see cref="Range{TValue}"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("[{0}, {1}]", LowerBound, UpperBound);
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
