using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents parameters used when launching compute shader work groups.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [TypeConverter(typeof(NumericRecordConverter))]
    public struct DispatchParameters : IEquatable<DispatchParameters>
    {
        /// <summary>
        /// The number of work groups to be launched in the X dimension.
        /// </summary>
        public int NumGroupsX;

        /// <summary>
        /// The number of work groups to be launched in the Y dimension.
        /// </summary>
        public int NumGroupsY;

        /// <summary>
        /// The number of work groups to be launched in the Z dimension.
        /// </summary>
        public int NumGroupsZ;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchParameters"/> structure
        /// using the specified number of compute work groups.
        /// </summary>
        /// <param name="numGroupsX">
        /// The number of work groups to be launched in the X dimension.
        /// </param>
        /// <param name="numGroupsY">
        /// The number of work groups to be launched in the Y dimension.
        /// </param>
        /// <param name="numGroupsZ">
        /// The number of work groups to be launched in the Z dimension.
        /// </param>
        public DispatchParameters(int numGroupsX, int numGroupsY, int numGroupsZ)
        {
            NumGroupsX = numGroupsX;
            NumGroupsY = numGroupsY;
            NumGroupsZ = numGroupsZ;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to the
        /// specified <see cref="DispatchParameters"/> structure.
        /// </summary>
        /// <param name="other">
        /// The <see cref="DispatchParameters"/> object to compare with this instance.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="other"/> represents
        /// the same parameter values as this instance; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(DispatchParameters other)
        {
            return NumGroupsX == other.NumGroupsX &&
                   NumGroupsY == other.NumGroupsY &&
                   NumGroupsZ == other.NumGroupsZ;
        }

        /// <summary>
        /// Returns a value indicating whether the specified object is a <see cref="DispatchParameters"/>
        /// structure with the same parameter values as this instance.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="DispatchParameters"/>
        /// structure and has the same parameter values as this structure; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is DispatchParameters && Equals((DispatchParameters)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = 53;
            hash += 97 * NumGroupsX.GetHashCode();
            hash += 97 * NumGroupsY.GetHashCode();
            hash += 97 * NumGroupsZ.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Creates a <see cref="string"/> representation of this
        /// <see cref="DispatchParameters"/> structure.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing all the parameter values of this
        /// <see cref="DispatchParameters"/> structure.
        /// </returns>
        public override string ToString()
        {
            return $"NumGroups({NumGroupsX}, {NumGroupsY}, {NumGroupsZ})";
        }

        /// <summary>
        /// Indicates whether two <see cref="DispatchParameters"/> structures are equal.
        /// </summary>
        /// <param name="left">
        /// The <see cref="DispatchParameters"/> structure on the left-hand side of the
        /// equality operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="DispatchParameters"/> structure on the right-hand side of the
        /// equality operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// have equal parameter values; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(DispatchParameters left, DispatchParameters right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether two <see cref="DispatchParameters"/> structures are different.
        /// </summary>
        /// <param name="left">
        /// The <see cref="DispatchParameters"/> structure on the left-hand side of the
        /// inequality operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="DispatchParameters"/> structure on the right-hand side of the
        /// inequality operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// differ in any of their parameter values; <see langword="false"/> if
        /// <paramref name="left"/> and <paramref name="right"/> are equal.
        /// </returns>
        public static bool operator !=(DispatchParameters left, DispatchParameters right)
        {
            return !left.Equals(right);
        }
    }
}
