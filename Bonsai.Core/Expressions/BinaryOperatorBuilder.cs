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
        Func<object> getter;
        Action<object> setter;
        WorkflowProperty property;
        static readonly MethodInfo initializePropertyMethod = typeof(BinaryOperatorBuilder).GetMethod("InitializeProperty", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Gets or sets the value which will be paired with elements of the observable
        /// sequence in case the sequence itself is not composed of paired elements.
        /// </summary>
        [Browsable(false)]
        public object Value
        {
            get
            {
                if (property == null) return null;
                return getter();
            }
            set
            {
                if (property == null && value != null)
                {
                    initializePropertyMethod.MakeGenericMethod(value.GetType()).Invoke(this, null);
                }

                if (property != null)
                {
                    setter(value);
                }
            }
        }

        internal WorkflowProperty Property
        {
            get { return property; }
        }

        void InitializeProperty<TValue>()
        {
            var typedProperty = new WorkflowProperty<TValue>();
            getter = () => typedProperty.Value;
            setter = value => typedProperty.Value = (TValue)value;
            property = typedProperty;
        }

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
                getter = null;
                setter = null;
                property = null;
                left = ExpressionHelper.MemberAccess(expression, "Item1");
                right = ExpressionHelper.MemberAccess(expression, "Item2");
                if (left.Type != right.Type && left.Type.IsPrimitive && right.Type.IsPrimitive)
                {
                    var comparison = CompareConversion(left.Type, right.Type, typeof(object));
                    if (comparison < 0) left = Expression.Convert(left, right.Type);
                    else right = Expression.Convert(right, left.Type);
                }
            }
            else if (expression.Type.IsPrimitive || expression.Type == typeof(string))
            {
                if (property == null || property.GetType().GetGenericArguments()[0] != expression.Type)
                {
                    initializePropertyMethod.MakeGenericMethod(expression.Type).Invoke(this, null);
                }

                left = expression;
                var valueProperty = Expression.Constant(property);
                right = Expression.Property(valueProperty, "Value");
            }
            else
            {
                getter = null;
                setter = null;
                property = null;
                throw new InvalidOperationException("The input to this binary operator must be an element pair.");
            }

            return BuildSelector(left, right);
        }
    }
}
