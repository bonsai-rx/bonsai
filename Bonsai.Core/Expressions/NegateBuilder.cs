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
    /// Represents an expression builder that applies an arithmetic negation operation
    /// on elements of an observable sequence.
    /// </summary>
    [XmlType("Negate", Namespace = Constants.XmlNamespace)]
    [Description("Applies an arithmetic negation operation on elements of an observable sequence.")]
    public class NegateBuilder : SelectBuilder
    {
        /// <summary>
        /// Returns the expression that applies an arithmetic negation operation on
        /// the specified input parameter to the selector result.
        /// </summary>
        /// <param name="expression">The input parameter to the selector.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies an arithmetic negation operation
        /// on the input parameter to the selector result.
        /// </returns>
        protected override Expression BuildSelector(Expression expression)
        {
            return Expression.Negate(expression);
        }
    }
}
