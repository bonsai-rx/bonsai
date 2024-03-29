﻿using System.ComponentModel;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that applies a bitwise left-shift operation
    /// on elements of an observable sequence.
    /// </summary>
    [XmlType("LeftShift", Namespace = Constants.XmlNamespace)]
    [Description("Applies a bitwise left-shift operation on elements of an observable sequence.")]
    public class LeftShiftBuilder : SelectBuilder
    {
        /// <summary>
        /// Gets or sets the number of positions by which to shift the bits of the input elements.
        /// </summary>
        [Description("The number of positions by which to shift the bits of the input elements.")]
        public int Value { get; set; }

        /// <summary>
        /// Returns the expression that applies a bitwise left-shift operation
        /// to the input parameter.
        /// </summary>
        /// <param name="expression">The input parameter to the selector.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies a bitwise left-shift operation
        /// to the input parameter.
        /// </returns>
        protected override Expression BuildSelector(Expression expression)
        {
            var builder = Expression.Constant(this);
            var value = Expression.Property(builder, nameof(Value));
            return Expression.LeftShift(expression, value);
        }
    }
}
