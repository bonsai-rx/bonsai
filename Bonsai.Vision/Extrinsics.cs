using OpenCV.Net;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents parameters that describe the camera extrinsic properties
    /// such as rotation and translation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct Extrinsics : IEquatable<Extrinsics>
    {
        /// <summary>
        /// The camera extrinsic rotations about the x-, y-, and z- axes of the
        /// reference coordinate system.
        /// </summary>
        public Point3d Rotation;

        /// <summary>
        /// The translation of the camera from the origin of the reference
        /// coordinate system.
        /// </summary>
        public Point3d Translation;

        /// <summary>
        /// Returns a value indicating whether this instance is equal to the
        /// specified <see cref="Extrinsics"/> structure.
        /// </summary>
        /// <param name="other">
        /// The <see cref="Extrinsics"/> object to compare with this instance.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="other"/> represents
        /// the same parameter values as this instance; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(Extrinsics other)
        {
            return Rotation == other.Rotation && Translation == other.Translation;
        }

        /// <summary>
        /// Returns a value indicating whether the specified object is an <see cref="Extrinsics"/>
        /// structure with the same parameter values as this <see cref="Extrinsics"/> structure.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is an <see cref="Extrinsics"/>
        /// structure and has the same parameter values as this structure; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Extrinsics extrinsics)
            {
                return Equals(extrinsics);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this <see cref="Extrinsics"/> structure.
        /// </summary>
        /// <returns>
        /// An integer value that specifies a hash value for this
        /// <see cref="Extrinsics"/> structure.
        /// </returns>
        public override int GetHashCode()
        {
            return Rotation.GetHashCode() ^ Translation.GetHashCode();
        }

        /// <summary>
        /// Creates a <see cref="string"/> representation of this
        /// <see cref="Extrinsics"/> structure.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> containing all the parameter values of this
        /// <see cref="Extrinsics"/> structure.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{{Rotation: {0}, Translation: {1}}}", Rotation, Translation);
        }

        /// <summary>
        /// Indicates whether two <see cref="Extrinsics"/> structures are equal.
        /// </summary>
        /// <param name="left">
        /// The <see cref="Extrinsics"/> structure on the left-hand side of the
        /// equality operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="Extrinsics"/> structure on the right-hand side of the
        /// equality operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// have equal parameter values; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(Extrinsics left, Extrinsics right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether two <see cref="Extrinsics"/> structures are different.
        /// </summary>
        /// <param name="left">
        /// The <see cref="Extrinsics"/> structure on the left-hand side of the
        /// inequality operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="Extrinsics"/> structure on the right-hand side of the
        /// inequality operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// differ in any of their parameter values; <see langword="false"/> if
        /// <paramref name="left"/> and <paramref name="right"/> are equal.
        /// </returns>
        public static bool operator !=(Extrinsics left, Extrinsics right)
        {
            return !left.Equals(right);
        }
    }
}
