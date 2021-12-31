using System;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents a workflow argument assignment. This class determines the index of a
    /// workflow connection and is used to specify the order of input connections to
    /// any given node.
    /// </summary>
    [TypeConverter("Bonsai.Design.ExpressionBuilderArgumentTypeConverter, Bonsai.Design")]
    public class ExpressionBuilderArgument : IEquatable<ExpressionBuilderArgument>, IComparable<ExpressionBuilderArgument>, IComparable
    {
        /// <summary>
        /// The prefix that starts every input argument name.
        /// </summary>
        public const string ArgumentNamePrefix = "Source";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderArgument"/> class.
        /// </summary>
        public ExpressionBuilderArgument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderArgument"/> class with
        /// the specified argument index.
        /// </summary>
        /// <param name="index">The zero-based index of the input argument.</param>
        public ExpressionBuilderArgument(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionBuilderArgument"/> class with
        /// the specified argument name.
        /// </summary>
        /// <param name="name">
        /// The name of the input argument. Arbitrary named arguments are not supported, so all
        /// names must start with the <see cref="ArgumentNamePrefix"/> followed by the one-based
        /// argument index.
        /// </param>
        public ExpressionBuilderArgument(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the zero-based index of the input argument.
        /// </summary>
        [XmlIgnore]
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the input argument. Arbitrary named arguments are not supported, so all
        /// names must start with the <see cref="ArgumentNamePrefix"/> followed by the one-based
        /// argument index.
        /// </summary>
        [XmlText]
        public string Name
        {
            get { return ArgumentNamePrefix + (Index + 1); }
            set
            {
                if (!value.StartsWith(ArgumentNamePrefix))
                {
                    throw new ArgumentException(string.Format("Argument name prefix must start with '{0}'.", ArgumentNamePrefix), nameof(value));
                }

                int index;
                var indexString = value.Substring(ArgumentNamePrefix.Length);
                if (string.IsNullOrEmpty(indexString)) index = 1;
                else if (!int.TryParse(indexString, out index))
                {
                    throw new ArgumentException("Argument name has an incorrect format.", nameof(value));
                }

                Index = index - 1;
            }
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// Less than zero means this object is less than the <paramref name="other"/>
        /// parameter. Zero means this object is equal to <paramref name="other"/>.
        /// Greater than zero means this object is greater than <paramref name="other"/>.
        /// </returns>
        public int CompareTo(ExpressionBuilderArgument other)
        {
            if (other is null)
            {
                return 1;
            }

            return Index.CompareTo(other.Index);
        }

        int IComparable.CompareTo(object obj)
        {
            var other = (ExpressionBuilderArgument)obj;
            return CompareTo(other);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is an instance of <see cref="ExpressionBuilderArgument"/>
        /// and its index equals the index value of this instance; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is ExpressionBuilderArgument argument) return Equals(argument);
            else return false;
        }

        /// <summary>
        /// Indicates whether the specified argument is equal to the current argument.
        /// </summary>
        /// <param name="other">The argument object to compare with the current argument.</param>
        /// <returns>
        /// <see langword="true"/> if the specified argument object is equal to the current argument;
        /// otherwise, <see langword="false"/>.
        public bool Equals(ExpressionBuilderArgument other)
        {
            if (other is null) return false;
            return Index.Equals(other.Index);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Tests whether two <see cref="ExpressionBuilderArgument"/> instances are equal.
        /// </summary>
        /// <param name="left">The <see cref="ExpressionBuilderArgument"/> instance on the left of the equality operator.</param>
        /// <param name="right">The <see cref="ExpressionBuilderArgument"/> instance on the right of the equality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> have equal index;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(ExpressionBuilderArgument left, ExpressionBuilderArgument right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Tests whether two <see cref="ExpressionBuilderArgument"/> instances are different.
        /// </summary>
        /// <param name="left">The <see cref="ExpressionBuilderArgument"/> instance on the left of the inequality operator.</param>
        /// <param name="right">The <see cref="ExpressionBuilderArgument"/> instance on the right of the inequality operator.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/> differ in index;
        /// <see langword="false"/> if <paramref name="left"/> and <paramref name="right"/> are equal.
        /// </returns>
        public static bool operator !=(ExpressionBuilderArgument left, ExpressionBuilderArgument right)
        {
            if (left is null) return right is object;
            return !left.Equals(right);
        }

        /// <summary>
        /// Tests whether an <see cref="ExpressionBuilderArgument"/> object is less than
        /// another object of the same type.
        /// </summary>
        /// <param name="left">
        /// The <see cref="ExpressionBuilderArgument"/> object on the left of the less than operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="ExpressionBuilderArgument"/> object on the right of the less than operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> has an index smaller than <paramref name="right"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator <(ExpressionBuilderArgument left, ExpressionBuilderArgument right)
        {
            if (left is null) return right is object;
            return left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Tests whether an <see cref="ExpressionBuilderArgument"/> object is greater than
        /// another object of the same type.
        /// </summary>
        /// <param name="left">
        /// The <see cref="ExpressionBuilderArgument"/> object on the left of the greater than operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="ExpressionBuilderArgument"/> object on the right of the greater than operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> has an index greater than <paramref name="right"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator >(ExpressionBuilderArgument left, ExpressionBuilderArgument right)
        {
            if (left is null) return false;
            return left.CompareTo(right) > 0;
        }
    }
}
