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
    /// Represents an expression builder that applies an equality comparison
    /// on paired elements of an observable sequence.
    /// </summary>
    [XmlType("Equal", Namespace = Constants.XmlNamespace)]
    [Description("Applies an equality comparison on paired elements of an observable sequence.")]
    public class EqualBuilder : BinaryOperatorBuilder
    {
        /// <summary>
        /// Returns the expression that applies an equality comparison
        /// to the left and right parameters.
        /// </summary>
        /// <param name="left">The left input parameter.</param>
        /// <param name="right">The right input parameter.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies an equality comparison
        /// to the left and right parameters.
        /// </returns>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            return Expression.Equal(left, right);
        }
    }
}
