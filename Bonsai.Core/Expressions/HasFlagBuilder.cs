using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that determines whether one or more bit fields
    /// are set in each element of an observable sequence.
    /// </summary>
    [XmlType("HasFlag", Namespace = Constants.XmlNamespace)]
    [Description("Determines whether one or more bit fields are set in each element of an observable sequence.")]
    public class HasFlagBuilder : BinaryOperatorBuilder
    {
        /// <inheritdoc/>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            if (!right.Type.IsEnum)
            {
                throw new InvalidOperationException("The flag expression must be an enum type.");
            }

            if (left.Type.IsEnum && left.Type != right.Type)
            {
                throw new InvalidOperationException("The input enum must be of the same type as the flag.");
            }

            var underlyingType = Enum.GetUnderlyingType(right.Type);
            right = Expression.Convert(right, underlyingType);
            if (left.Type != underlyingType && (left.Type.IsEnum || left.Type.IsPrimitive))
            {
                left = Expression.Convert(left, underlyingType);
            }

            var defaultValue = Expression.Default(underlyingType);
            return Expression.NotEqual(Expression.And(left, right), defaultValue);
        }

        /// <inheritdoc/>
        protected override Expression BuildSelector(Expression expression)
        {
            Expression left, right;
            var expressionTypeDefinition = expression.Type.IsGenericType ? expression.Type.GetGenericTypeDefinition() : null;
            if (expressionTypeDefinition == typeof(Tuple<,>))
            {
                Operand = null;
                left = ExpressionHelper.MemberAccess(expression, "Item1");
                right = ExpressionHelper.MemberAccess(expression, "Item2");
            }
            else
            {
                var operand = Operand;
                if (operand == null)
                {
                    if (!expression.Type.IsEnum)
                    {
                        throw new InvalidOperationException("The input expression must be an enum type.");
                    }

                    var propertyType = GetWorkflowPropertyType(expression.Type);
                    Operand = operand = (WorkflowProperty)Activator.CreateInstance(propertyType);
                }

                left = expression;
                var operandExpression = Expression.Constant(operand);
                right = Expression.Property(operandExpression, "Value");
            }

            return BuildSelector(left, right);
        }
    }
}
