using Bonsai.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that applies an index operator to
    /// the elements of an observable sequence.
    /// </summary>
    [XmlType("Index", Namespace = Constants.XmlNamespace)]
    [Description("Applies an index operator to the elements of an observable sequence.")]
    public class IndexBuilder : BinaryOperatorBuilder
    {
        static Type GetDefaultOperandType(Expression expression)
        {
            if (expression.Type.IsArray) return typeof(int);
            else
            {
                var parameter = (from member in expression.Type.GetDefaultMembers().OfType<PropertyInfo>()
                                 let parameters = member.GetIndexParameters()
                                 where parameters.Length == 1
                                 select parameters[0])
                                 .FirstOrDefault();
                if (parameter == null)
                {
                    throw new InvalidOperationException(string.Format(Resources.Exception_IndexerNotFound, expression.Type));
                }

                return parameter.ParameterType;
            }
        }

        /// <summary>
        /// Returns the expression that applies an index operator to
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
            if (left.Type.IsArray) return Expression.ArrayIndex(left, right);
            else
            {
                var indexer = (from member in left.Type.GetDefaultMembers().OfType<PropertyInfo>()
                               let parameters = member.GetIndexParameters()
                               where parameters.Length == 1
                               select member)
                               .FirstOrDefault();
                return Expression.Property(left, indexer, right);
            }
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
                var operandType = GetDefaultOperandType(expression);
                if (operand == null || operand.PropertyType != operandType)
                {
                    if (operandType.IsInterface ||
                        operandType.IsClass && operandType.GetConstructor(Type.EmptyTypes) == null)
                    {
                        throw new InvalidOperationException(string.Format(Resources.Exception_UnsupportedMinArgumentCount, 2));
                    }

                    var propertyType = GetWorkflowPropertyType(operandType);
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
