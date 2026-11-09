using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using Bonsai.Properties;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder for an operator that determines the existence of a specific key,
    /// and returns its value, if it exists.
    /// </summary>
    [XmlType("TryGetValue", Namespace = Constants.XmlNamespace)]
    [Description("Applies an operator to an observable sequence, that determines the existence of a specific key, and returns its value, if it exists.")]
    public class TryGetValueBuilder : BinaryOperatorBuilder
    {
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
            Expression left, right;
            var expressionTypeDefinition = expression.Type.IsGenericType ? expression.Type.GetGenericTypeDefinition() : null;
            if (expressionTypeDefinition == typeof(Tuple<,>))
            {
                Operand = null;
                left = ExpressionHelper.MemberAccess(expression, "Item1");
                right = ExpressionHelper.MemberAccess(expression, "Item2");
                return BuildSelector(left, right);
            }
            else
            {
                var operand = Operand;
                var operandType = ExpressionHelper.GetIndexerTypes(expression, 1)[0];
                if (operand == null || operand.PropertyType != operandType)
                {
                    var propertyType = GetWorkflowPropertyType(operandType);
                    try
                    {
                        Operand = operand = (WorkflowProperty)Activator.CreateInstance(propertyType);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            string.Format(Resources.Exception_UnsupportedMinArgumentCount, 2),
                            ex);
                    }
                }

                left = expression;
                var operandExpression = Expression.Constant(operand);
                right = ExpressionHelper.Property(operandExpression, "Value");
            }

            return BuildSelector(left, right);
        }

        /// <summary>
        /// Returns the expression that applies the operator to
        /// the left and right parameters.
        /// </summary>
        /// <param name="left">The left input parameter.</param>
        /// <param name="right">The right input parameter.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies a TryGetValue operation to
        /// the left and right parameters.
        /// </returns>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            // We could grab the TryGetValue method directly, but we need the indexer type anyway
            PropertyInfo indexer = left.Type.GetProperty("Item", new[] { right.Type });

            if (indexer == null)
            {
                throw new Exception(
                    $"Type {left.Type.Name} does not support indexing with {right.Type.Name}"
                );
            }

            var valueType = indexer.PropertyType;

            MethodInfo tryGetValueMethod = left.Type.GetMethod(
                "TryGetValue",
                new[] { right.Type, valueType.MakeByRefType() }
            );

            if (tryGetValueMethod == null)
            {
                throw new Exception(
                    $"Type {left.Type.Name} does not support TryGetValue({right.Type.Name}, out {valueType.Name})"
                );
            }

            var outputTupleType = typeof(Tuple<,>).MakeGenericType(typeof(bool), valueType);
            var outputConstructor = outputTupleType.GetConstructor(new[] { typeof(bool), valueType });

            var valueVariable = Expression.Variable(valueType, "value");

            return Expression.Block(
                outputTupleType,
                new[] { valueVariable },
                Expression.New(
                    outputConstructor,
                    Expression.Call(left, tryGetValueMethod, right, valueVariable),
                    valueVariable
                )
            );
        }
    }
}
