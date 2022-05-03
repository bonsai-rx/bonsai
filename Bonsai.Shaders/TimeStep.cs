using System;
using System.Collections.Generic;
using System.Globalization;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents the amount of time elapsed since the last update.
    /// </summary>
    public struct TimeStep : IEquatable<TimeStep>
    {
        /// <summary>
        /// Represents the zero <see cref="TimeStep"/> value. This field is read-only.
        /// </summary>
        public static readonly TimeStep Zero = new TimeStep();

        /// <summary>
        /// The amount of elapsed time since the last update, in seconds.
        /// </summary>
        /// <remarks>
        /// This field is useful for fixed-step deterministic state updates, where
        /// each step follows the target update or render refresh rates.
        /// </remarks>
        public double ElapsedTime;

        /// <summary>
        /// The amount of elapsed time since the last update, in seconds, following
        /// the host computer clock.
        /// </summary>
        /// <remarks>
        /// This field can be used to measure the real-time jitter of the update
        /// and render loops, or in variable-step state updates.
        /// </remarks>
        public double ElapsedRealTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeStep"/> structure
        /// using the specified fixed-step elapsed time and variable-step elapsed
        /// clock time.
        /// </summary>
        /// <param name="elapsedTime">
        /// The amount of elapsed time since the last update, in seconds.
        /// </param>
        /// <param name="elapsedRealTime">
        /// The amount of elapsed time since the last update, in seconds, following
        /// the host computer clock.
        /// </param>
        public TimeStep(double elapsedTime, double elapsedRealTime)
        {
            ElapsedTime = elapsedTime;
            ElapsedRealTime = elapsedRealTime;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to the
        /// specified <see cref="TimeStep"/> structure.
        /// </summary>
        /// <param name="other">
        /// The <see cref="TimeStep"/> object to compare with this instance.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="other"/> represents
        /// the same timing values as this instance; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(TimeStep other)
        {
            return ElapsedTime == other.ElapsedTime && ElapsedRealTime == other.ElapsedRealTime;
        }

        /// <summary>
        /// Returns a value indicating whether the specified object is a <see cref="TimeStep"/>
        /// structure with the same timing values as this <see cref="TimeStep"/> object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="obj"/> is a <see cref="TimeStep"/>
        /// structure and has the same timing values as this object; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is TimeStep step)
            {
                return Equals(step);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this <see cref="TimeStep"/> value.
        /// </summary>
        /// <returns>
        /// An integer value that specifies a hash value for this
        /// <see cref="TimeStep"/> object.
        /// </returns>
        public override int GetHashCode()
        {
            var hash = 82613;
            hash = hash * 103787 + EqualityComparer<double>.Default.GetHashCode(ElapsedTime);
            hash = hash * 103787 + EqualityComparer<double>.Default.GetHashCode(ElapsedRealTime);
            return hash;
        }

        /// <summary>
        /// Creates a <see cref="string"/> representation of this
        /// <see cref="TimeStep"/> value.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> representing all the timing values of this
        /// <see cref="TimeStep"/> object.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{ElapsedTime: {0}, ElapsedRealTime: {1}}}",
                ElapsedTime, ElapsedRealTime);
        }

        /// <summary>
        /// Adds two <see cref="TimeStep"/> values together and stores the result
        /// in a return value.
        /// </summary>
        /// <param name="left">The first <see cref="TimeStep"/> to add.</param>
        /// <param name="right">The second <see cref="TimeStep"/> to add.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="TimeStep"/> object
        /// representing the sum of the <paramref name="left"/> and
        /// <paramref name="right"/> values.
        /// </param>
        public static void Add(ref TimeStep left, ref TimeStep right, out TimeStep result)
        {
            result.ElapsedTime = left.ElapsedTime + right.ElapsedTime;
            result.ElapsedRealTime = left.ElapsedRealTime + right.ElapsedRealTime;
        }

        /// <summary>
        /// Subtracts the second <see cref="TimeStep"/> value from the first
        /// and stores the result in a return value.
        /// </summary>
        /// <param name="left">The first <see cref="TimeStep"/> value.</param>
        /// <param name="right">The second <see cref="TimeStep"/> value.</param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="TimeStep"/> object
        /// representing the difference between the <paramref name="left"/> and
        /// <paramref name="right"/> values.
        /// </param>
        public static void Subtract(ref TimeStep left, ref TimeStep right, out TimeStep result)
        {
            result.ElapsedTime = left.ElapsedTime - right.ElapsedTime;
            result.ElapsedRealTime = left.ElapsedRealTime - right.ElapsedRealTime;
        }

        /// <summary>
        /// Adds two <see cref="TimeStep"/> values together.
        /// </summary>
        /// <param name="left">The first <see cref="TimeStep"/> to add.</param>
        /// <param name="right">The second <see cref="TimeStep"/> to add.</param>
        /// <returns>
        /// A new <see cref="TimeStep"/> object representing the sum of the
        /// <paramref name="left"/> and <paramref name="right"/> values.
        /// </returns>
        public static TimeStep Add(TimeStep left, TimeStep right)
        {
            Add(ref left, ref right, out TimeStep result);
            return result;
        }

        /// <summary>
        /// Subtracts the second <see cref="TimeStep"/> value from the first.
        /// </summary>
        /// <param name="left">The first <see cref="TimeStep"/> value.</param>
        /// <param name="right">The second <see cref="TimeStep"/> value.</param>
        /// <returns>
        /// A new <see cref="TimeStep"/> object representing the difference between
        /// the <paramref name="left"/> and <paramref name="right"/> values.
        /// </returns>
        public static TimeStep Subtract(TimeStep left, TimeStep right)
        {
            Subtract(ref left, ref right, out TimeStep result);
            return result;
        }

        /// <summary>
        /// Indicates whether two <see cref="TimeStep"/> values are equal.
        /// </summary>
        /// <param name="left">
        /// The <see cref="TimeStep"/> value on the left-hand side of the
        /// equality operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="TimeStep"/> value on the right-hand side of the
        /// equality operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// have equal timing values; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(TimeStep left, TimeStep right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Indicates whether two <see cref="TimeStep"/> values are different.
        /// </summary>
        /// <param name="left">
        /// The <see cref="TimeStep"/> value on the left-hand side of the
        /// inequality operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="TimeStep"/> value on the right-hand side of the
        /// inequality operator.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> and <paramref name="right"/>
        /// differ in any of their timing values; <see langword="false"/> if
        /// <paramref name="left"/> and <paramref name="right"/> are equal.
        /// </returns>
        public static bool operator !=(TimeStep left, TimeStep right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Adds two <see cref="TimeStep"/> values together.
        /// </summary>
        /// <param name="left">
        /// The <see cref="TimeStep"/> value on the left-hand side of the
        /// addition operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="TimeStep"/> value on the right-hand side of the
        /// addition operator.
        /// </param>
        /// <returns>
        /// A new <see cref="TimeStep"/> object representing the sum of the
        /// <paramref name="left"/> and <paramref name="right"/> values.
        /// </returns>
        public static TimeStep operator +(TimeStep left, TimeStep right)
        {
            return Add(left, right);
        }

        /// <summary>
        /// Subtracts the second <see cref="TimeStep"/> value from the first.
        /// </summary>
        /// <param name="left">
        /// The <see cref="TimeStep"/> value on the left-hand side of the
        /// subtraction operator.
        /// </param>
        /// <param name="right">
        /// The <see cref="TimeStep"/> value on the right-hand side of the
        /// subtraction operator.
        /// </param>
        /// <returns>
        /// A new <see cref="TimeStep"/> object representing the difference between
        /// the <paramref name="left"/> and <paramref name="right"/> values.
        /// </returns>
        public static TimeStep operator -(TimeStep left, TimeStep right)
        {
            return Subtract(left, right);
        }
    }
}
