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
    /// Represents an expression builder that applies a bitwise complement operation
    /// on elements of an observable sequence.
    /// </summary>
    [XmlType("BitwiseNot", Namespace = Constants.XmlNamespace)]
    [Description("Applies a bitwise complement operation on elements of an observable sequence.")]
    public class BitwiseNotBuilder : SelectBuilder
    {
        /// <summary>
        /// Returns the expression that applies a bitwise complement operation on
        /// the specified input parameter to the selector result.
        /// </summary>
        /// <param name="expression">The input parameter to the selector.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies a bitwise complement operation
        /// on the input parameter to the selector result.
        /// </returns>
        protected override Expression BuildSelector(Expression expression)
        {
            return Expression.Not(expression);
        }
    }
}
