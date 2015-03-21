using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents the current state of an accumulation over an observable sequence.
    /// </summary>
    /// <typeparam name="TAccumulation">The type of values in the accumulation.</typeparam>
    /// <typeparam name="TElement">The type of values in the observable sequence.</typeparam>
    public class ElementAccumulation<TAccumulation, TElement> : IStructuralEquatable, IStructuralComparable, IComparable
    {
        readonly TAccumulation accumulation;
        readonly TElement value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementAccumulation{A,T}"/> class
        /// with the specified accumulator state and current element information.
        /// </summary>
        /// <param name="accumulation">The current state of the accumulator.</param>
        /// <param name="value">The current value of the sequence to accumulate.</param>
        public ElementAccumulation(TAccumulation accumulation, TElement value)
        {   
            this.accumulation = accumulation;
            this.value = value;
        }

        /// <summary>
        /// Gets the current state of the accumulator.
        /// </summary>
        public TAccumulation Accumulation
        {
            get { return accumulation; }
        }

        /// <summary>
        /// Gets the current value of the sequence to accumulate.
        /// </summary>
        public TElement Value
        {
            get { return value; }
        }

        /// <summary>
        /// Returns a value that indicates whether the current <see cref="ElementAccumulation{A,T}"/>
        /// object is equal to a specified object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        /// <b>true</b> if the current instance is equal to the specified object;
        /// otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return ((IStructuralEquatable)this).Equals(obj, EqualityComparer<object>.Default);
        }

        bool IStructuralEquatable.Equals(object other, IEqualityComparer comparer)
        {
            if (other == null)
            {
                return false;
            }

            var accumulator = other as ElementAccumulation<TAccumulation, TElement>;
            if (accumulator == null)
            {
                return false;
            }

            return comparer.Equals(accumulation, accumulator.accumulation) &&
                   comparer.Equals(value, accumulator.value);
        }

        /// <summary>
        /// Returns the hash code for the current <see cref="ElementAccumulation{A,T}"/> object.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return ((IStructuralEquatable)this).GetHashCode(EqualityComparer<object>.Default);
        }

        int IStructuralEquatable.GetHashCode(IEqualityComparer comparer)
        {
            var hash = 23;
            hash = hash * 7919 + comparer.GetHashCode(accumulation);
            hash = hash * 7919 + comparer.GetHashCode(value);
            return hash;
        }

        int IComparable.CompareTo(object obj)
        {
            return ((IStructuralComparable)this).CompareTo(obj, Comparer<object>.Default);
        }

        int IStructuralComparable.CompareTo(object other, IComparer comparer)
        {
            if (other == null)
            {
                return 1;
            }

            var accumulator = other as ElementAccumulation<TAccumulation, TElement>;
            if (accumulator == null)
            {
                throw new ArgumentException(string.Format("Argument must be of type {0}.", GetType()), "other");
            }

            var result = comparer.Compare(accumulation, accumulator.accumulation);
            if (result != 0) return result;
            return comparer.Compare(value, accumulator.value);
        }

        /// <summary>
        /// Returns a string that represents the current <see cref="ElementAccumulation{A,T}"/>
        /// instance.
        /// </summary>
        /// <returns>
        /// The string representation of this <see cref="ElementAccumulation{A,T}"/> object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("(A:{0}, V:{1})", accumulation, value);
        }
    }
}
