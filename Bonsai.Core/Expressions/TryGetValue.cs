using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using Bonsai.Properties;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder for an operator that determines the existance of a specific key,
    /// and returns its value, if it exists.
    /// </summary>
    [XmlType("Value", Namespace = Constants.XmlNamespace)]
    [Description("Applies an operator to an observable sequence, that determines the existance of a specific key, and returns its value, if it exists.")]
    public class TryGetValue : BinaryOperatorBuilder
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
        /// The <see cref="Expression"/> that applies an index operator to
        /// the left and right parameters.
        /// </returns>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            MethodInfo containsKeyMethod =
                left.Type.GetMethod("ContainsKey", new[] { right.Type });

            PropertyInfo indexer =
                left.Type.GetProperty("Item", new[] { right.Type });

            if (containsKeyMethod == null || indexer == null)
            {
                throw new Exception(
                    $"Type {left.Type.Name} does not support ContainsKey({right.Type.Name}) with an indexer"
                );
            }

            var valueType = indexer.PropertyType;

            var outputTupleType = typeof(Tuple<,>).MakeGenericType(typeof(bool), valueType);
            var outputConstructor = outputTupleType.GetConstructor(new[] { typeof(bool), valueType });


            return Expression.Condition(
                Expression.Call(left, containsKeyMethod, right),
                MakeTuple(outputConstructor, Expression.Constant(true), Expression.Property(left, indexer, right)),
                MakeTuple(outputConstructor, Expression.Constant(false), Expression.Default(valueType))
            );
        }

        private static Expression MakeTuple(
            ConstructorInfo constructor,
            Expression resultFlag,
            Expression outputValue)
        {
            return Expression.New(constructor, resultFlag, outputValue);
        }
    }
}
