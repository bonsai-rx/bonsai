using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that applies a bitwise right-shift operation
    /// on elements of an observable sequence.
    /// </summary>
    [XmlType("RightShift", Namespace = Constants.XmlNamespace)]
    [Description("Applies a bitwise right-shift operation on elements of an observable sequence.")]
    public class RightShiftBuilder : SelectBuilder
    {
        /// <summary>
        /// Gets or sets the number of positions by which to shift the bits of the input elements.
        /// </summary>
        [Description("The number of positions by which to shift the bits of the input elements.")]
        public int Value { get; set; }

        /// <summary>
        /// Returns the expression that applies a bitwise right-shift operation
        /// to the input parameter.
        /// </summary>
        /// <param name="expression">The input parameter to the selector.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies a bitwise right-shift operation
        /// to the input parameter.
        /// </returns>
        protected override Expression BuildSelector(Expression expression)
        {
            var builder = Expression.Constant(this);
            var value = Expression.Property(builder, "Value");
            return Expression.RightShift(expression, value);
        }
    }
}
