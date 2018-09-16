using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Specifies the valid range of values for a numeric property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RangeAttribute : Attribute
    {
        /// <summary>
        /// Specifies the default value for the <see cref="RangeAttribute"/>. This field is read-only.
        /// </summary>
        public static readonly RangeAttribute Default = new RangeAttribute(decimal.MinValue, decimal.MaxValue);

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeAttribute"/> class with the
        /// specified minimum and maximum values.
        /// </summary>
        /// <param name="min">An <see cref="Int32"/> that is the minimum value.</param>
        /// <param name="max">An <see cref="Int32"/> that is the maximum value.</param>
        public RangeAttribute(int min, int max)
            : this((decimal)min, (decimal)max)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeAttribute"/> class with the
        /// specified minimum and maximum values.
        /// </summary>
        /// <param name="min">An <see cref="Int64"/> that is the minimum value.</param>
        /// <param name="max">An <see cref="Int64"/> that is the maximum value.</param>
        public RangeAttribute(long min, long max)
            : this((decimal)min, (decimal)max)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeAttribute"/> class with the
        /// specified minimum and maximum values.
        /// </summary>
        /// <param name="min">A <see cref="Single"/> that is the minimum value.</param>
        /// <param name="max">A <see cref="Single"/> that is the maximum value.</param>
        public RangeAttribute(float min, float max)
            : this((decimal)min, (decimal)max)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeAttribute"/> class with the
        /// specified minimum and maximum values.
        /// </summary>
        /// <param name="min">A <see cref="Double"/> that is the minimum value.</param>
        /// <param name="max">A <see cref="Double"/> that is the maximum value.</param>
        public RangeAttribute(double min, double max)
            : this((decimal)min, (decimal)max)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeAttribute"/> class with the
        /// specified minimum and maximum values.
        /// </summary>
        /// <param name="min">A <see cref="Decimal"/> that is the minimum value.</param>
        /// <param name="max">A <see cref="Decimal"/> that is the maximum value.</param>
        public RangeAttribute(decimal min, decimal max)
        {
            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        /// Gets the minimum value of the property this attribute is bound to.
        /// </summary>
        public decimal Minimum { get; private set; }

        /// <summary>
        /// Gets the maximum value of the property this attribute is bound to.
        /// </summary>
        public decimal Maximum { get; private set; }
    }
}
