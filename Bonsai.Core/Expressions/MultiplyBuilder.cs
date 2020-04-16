using System.ComponentModel;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that applies the arithmetic multiplication operation
    /// on paired elements of an observable sequence.
    /// </summary>
    [XmlType("Multiply", Namespace = Constants.XmlNamespace)]
    [Description("Applies the arithmetic multiplication operation on paired elements of an observable sequence.")]
    public class MultiplyBuilder : BinaryOperatorBuilder
    {
        /// <summary>
        /// Returns the expression that applies the arithmetic multiplication operation
        /// to the left and right parameters.
        /// </summary>
        /// <param name="left">The left input parameter.</param>
        /// <param name="right">The right input parameter.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies the arithmetic multiplication operation
        /// to the left and right parameters.
        /// </returns>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            return Expression.Multiply(left, right);
        }
    }
}
