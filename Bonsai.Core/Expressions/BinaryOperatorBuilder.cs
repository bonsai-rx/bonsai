using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that define a simple binary operator
    /// on paired elements of an observable sequence. This is an abstract class.
    /// </summary>
    [TypeDescriptionProvider(typeof(BinaryOperatorTypeDescriptionProvider))]
    public abstract class BinaryOperatorBuilder : SelectBuilder
    {
        /// <summary>
        /// Gets or sets the value which will be paired with elements of the observable
        /// sequence in case the sequence itself is not composed of paired elements.
        /// </summary>
        [Browsable(false)]
        public WorkflowProperty Operand { get; set; }

        /// <summary>
        /// When overridden in a derived class, returns the expression that applies a binary
        /// operator to the left and right parameters.
        /// </summary>
        /// <param name="left">The left input parameter.</param>
        /// <param name="right">The right input parameter.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies the binary operator to the left
        /// and right parameters.
        /// </returns>
        protected abstract Expression BuildSelector(Expression left, Expression right);

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
            if (expression.Type.IsGenericType && expression.Type.GetGenericTypeDefinition() == typeof(Tuple<,>))
            {
                Operand = null;
                left = ExpressionHelper.MemberAccess(expression, "Item1");
                right = ExpressionHelper.MemberAccess(expression, "Item2");
                if (left.Type != right.Type && left.Type.IsPrimitive && right.Type.IsPrimitive)
                {
                    var comparison = CompareConversion(left.Type, right.Type, typeof(object));
                    if (comparison < 0) left = Expression.Convert(left, right.Type);
                    else right = Expression.Convert(right, left.Type);
                }
            }
            else
            {
                var operand = Operand;
                if (operand == null || operand.PropertyType != expression.Type)
                {
                    var propertyType = GetPropertyType(expression.Type);
                    Operand = operand = (WorkflowProperty)Activator.CreateInstance(propertyType);
                }

                left = expression;
                var operandExpression = Expression.Constant(operand);
                right = Expression.Property(operandExpression, "Value");
            }

            return BuildSelector(left, right);
        }

        Type GetPropertyType(Type expressionType)
        {
            if (expressionType == typeof(DateTimeOffset)) return typeof(DateTimeOffsetProperty);
            if (expressionType == typeof(TimeSpan)) return typeof(TimeSpanProperty);

            var typeCode = Type.GetTypeCode(expressionType);
            switch (typeCode)
            {
                case TypeCode.Boolean: return typeof(BooleanProperty);
                case TypeCode.DateTime: return typeof(DateTimeProperty);
                case TypeCode.Double: return typeof(DoubleProperty);
                case TypeCode.Int32: return typeof(IntProperty);
                case TypeCode.Single: return typeof(FloatProperty);
                case TypeCode.String: return typeof(StringProperty);
                case TypeCode.Byte:
                case TypeCode.Char:
                case TypeCode.DBNull:
                case TypeCode.Decimal:
                case TypeCode.Empty:
                case TypeCode.Int16:
                case TypeCode.Int64:
                case TypeCode.Object:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                default:
                    return typeof(WorkflowProperty<>).MakeGenericType(expressionType);
            }
        }
    }
}
