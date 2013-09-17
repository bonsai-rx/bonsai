using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public static class Range
    {
        public static Range<T> Create<T>(T lowerBound, T upperBound)
        {
            return new Range<T>(lowerBound, upperBound);
        }

        public static Range<T> Create<T>(T lowerBound, T upperBound, IComparer<T> comparer)
        {
            return new Range<T>(lowerBound, upperBound, comparer);
        }
    }

    public sealed class Range<T>
    {
        readonly IComparer<T> valueComparer;

        public Range(T lowerBound, T upperBound)
            : this(lowerBound, upperBound, Comparer<T>.Default)
        {
        }

        public Range(T lowerBound, T upperBound, IComparer<T> comparer)
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

        public T LowerBound { get; private set; }

        public T UpperBound { get; private set; }

        public bool Contains(T value)
        {
            return valueComparer.Compare(LowerBound, value) <= 0 &&
                valueComparer.Compare(value, UpperBound) <= 0;
        }
    }
}
