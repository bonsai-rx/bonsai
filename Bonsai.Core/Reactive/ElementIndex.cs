using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that incorporates the zero-based index of elements
    /// into an observable sequence.
    /// </summary>
    [Combinator]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Incorporates the zero-based index of elements into an observable sequence.")]
    public class ElementIndex
    {
        /// <summary>
        /// Incorporates the zero-based index of elements into an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence for which to incorporate element indices.</param>
        /// <returns>An observable sequence with index information on elements.</returns>
        public IObservable<ElementIndex<TSource>> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select((value, index) => new ElementIndex<TSource>(value, index));
        }
    }

    /// <summary>
    /// Represents an element from an observable sequence associated with its index information.
    /// The zero-based index represents the order of the element in the sequence.
    /// </summary>
    /// <typeparam name="T">The type of the value being annotated with index information.</typeparam>
    public struct ElementIndex<T> : IEquatable<ElementIndex<T>>
    {
        readonly int index;
        readonly T value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementIndex{T}"/> class with the specified
        /// value and index information.
        /// </summary>
        /// <param name="value">The value to be annotated with index information.</param>
        /// <param name="index">The zero-based index of the element in the sequence.</param>
        public ElementIndex(T value, int index)
        {
            this.value = value;
            this.index = index;
        }

        /// <summary>
        /// Gets the zero-based index of the element in the sequence.
        /// </summary>
        public int Index
        {
            get { return index; }
        }

        /// <summary>
        /// Gets the value of the element.
        /// </summary>
        public T Value
        {
            get { return value; }
        }

        /// <summary>
        /// Returns a value indicating whether this instance has the same value and index
        /// as a specified <see cref="ElementIndex{T}"/> structure.
        /// </summary>
        /// <param name="other">The <see cref="ElementIndex{T}"/> structure to compare to this instance.</param>
        /// <returns>
        /// <b>true</b> if <paramref name="other"/> has the same value and index as this
        /// instance; otherwise, <b>false</b>.
        /// </returns>
        public bool Equals(ElementIndex<T> other)
        {
            return index == other.Index && EqualityComparer<T>.Default.Equals(value, other.Value);
        }

        /// <summary>
        /// Tests to see whether the specified object is an <see cref="ElementIndex{T}"/> structure
        /// with the same value and index as this <see cref="ElementIndex{T}"/> structure.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to test.</param>
        /// <returns>
        /// <b>true</b> if <paramref name="obj"/> is an <see cref="ElementIndex{T}"/> and has the
        /// same value and index as this <see cref="ElementIndex{T}"/>; otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is ElementIndex<T>)
            {
                return Equals((ElementIndex<T>)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this <see cref="ElementIndex{T}"/> structure.
        /// </summary>
        /// <returns>An integer value that specifies a hash value for this <see cref="ElementIndex{T}"/> structure.</returns>
        public override int GetHashCode()
        {
            return index.GetHashCode() ^ EqualityComparer<T>.Default.GetHashCode(value);
        }

        /// <summary>
        /// Creates a <see cref="String"/> representation of this <see cref="ElementIndex{T}"/>
        /// structure.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> containing the <see cref="Value"/> and <see cref="Index"/>
        /// properties of this <see cref="ElementIndex{T}"/> structure.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}@{1}", value, index);
        }

        /// <summary>
        /// Tests whether two <see cref="ElementIndex{T}"/> structures are equal.
        /// </summary>
        /// <param name="left">The <see cref="ElementIndex{T}"/> structure on the left of the equality operator.</param>
        /// <param name="right">The <see cref="ElementIndex{T}"/> structure on the right of the equality operator.</param>
        /// <returns>
        /// <b>true</b> if <paramref name="left"/> and <paramref name="right"/> have equal value and index;
        /// otherwise, <b>false</b>.
        /// </returns>
        public static bool operator ==(ElementIndex<T> left, ElementIndex<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Tests whether two <see cref="ElementIndex{T}"/> structures are different.
        /// </summary>
        /// <param name="left">The <see cref="ElementIndex{T}"/> structure on the left of the inequality operator.</param>
        /// <param name="right">The <see cref="ElementIndex{T}"/> structure on the right of the inequality operator.</param>
        /// <returns>
        /// <b>true</b> if <paramref name="left"/> and <paramref name="right"/> differ either in value or index;
        /// <b>false</b> if <paramref name="left"/> and <paramref name="right"/> are equal.
        /// </returns>
        public static bool operator !=(ElementIndex<T> left, ElementIndex<T> right)
        {
            return !left.Equals(right);
        }
    }
}
