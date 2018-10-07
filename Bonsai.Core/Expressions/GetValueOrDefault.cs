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
    /// Represents an expression builder that retrieves the value of each nullable
    /// element in the sequence, or the specified default value.
    /// </summary>
    [XmlType("GetValueOrDefault", Namespace = Constants.XmlNamespace)]
    [Description("Retrieves the value of each nullable element in the sequence, or the specified default value.")]
    public class GetValueOrDefaultBuilder : BinaryOperatorBuilder
    {
        /// <summary>
        /// Returns the expression that retrieves the value of the left parameter,
        /// or the default value specified by the right parameter.
        /// </summary>
        /// <param name="left">The left input parameter.</param>
        /// <param name="right">The right input parameter.</param>
        /// <returns>
        /// The <see cref="Expression"/> that retrieves the value of the left parameter,
        /// or the default value specified by the right parameter.
        /// </returns>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            return Expression.Call(left, "GetValueOrDefault", null, right);
        }

        /// <summary>
        /// Returns the expression that maps the specified input parameter to the selector result.
        /// </summary>
        /// <param name="expression">The input parameter to the selector.</param>
        /// <returns>
        /// The <see cref="Expression"/> that maps the input parameter to the
        /// selector result.
        /// </returns>
        protected override Expression BuildSelector(Expression expression)
        {
            var nullableType = Nullable.GetUnderlyingType(expression.Type);
            if (nullableType == null)
            {
                throw new InvalidOperationException("The input element type is not nullable.");
            }

            var operand = Operand;
            if (operand == null || operand.PropertyType != nullableType)
            {
                var propertyType = GetWorkflowPropertyType(nullableType);
                Operand = operand = (WorkflowProperty)Activator.CreateInstance(propertyType);
            }

            var left = expression;
            var operandExpression = Expression.Constant(operand);
            var right = Expression.Property(operandExpression, "Value");
            return BuildSelector(left, right);
        }
    }
}
