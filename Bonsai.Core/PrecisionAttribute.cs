using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Specifies the number of decimal places and the smallest incremental step that
    /// should be used when editing values for a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PrecisionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrecisionAttribute"/> with the
        /// specified number of decimal places and the smallest editor step increment.
        /// </summary>
        /// <param name="decimalPlaces">
        /// The number of decimal places to display in the editor.
        /// </param>
        /// <param name="increment">
        /// The <see cref="Int32"/> value by which to increment or decrement the current
        /// value on each editor step.
        /// </param>
        public PrecisionAttribute(int decimalPlaces, int increment)
            : this(decimalPlaces, (decimal)increment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrecisionAttribute"/> with the
        /// specified number of decimal places and the smallest editor step increment.
        /// </summary>
        /// <param name="decimalPlaces">
        /// The number of decimal places to display in the editor.
        /// </param>
        /// <param name="increment">
        /// The <see cref="Double"/> value by which to increment or decrement the current
        /// value on each editor step.
        /// </param>
        public PrecisionAttribute(int decimalPlaces, double increment)
            : this(decimalPlaces, (decimal)increment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrecisionAttribute"/> with the
        /// specified number of decimal places and the smallest editor step increment.
        /// </summary>
        /// <param name="decimalPlaces">
        /// The number of decimal places to display in the editor.
        /// </param>
        /// <param name="increment">
        /// The <see cref="Decimal"/> value by which to increment or decrement the current
        /// value on each editor step.
        /// </param>
        public PrecisionAttribute(int decimalPlaces, decimal increment)
        {
            DecimalPlaces = decimalPlaces;
            Increment = increment;
        }

        /// <summary>
        /// Gets the number of decimal places to display in the editor.
        /// </summary>
        public int DecimalPlaces { get; private set; }

        /// <summary>
        /// Gets the smallest value by which to increment or decrement the current value
        /// on each editor step.
        /// </summary>
        public decimal Increment { get; private set; }
    }
}
