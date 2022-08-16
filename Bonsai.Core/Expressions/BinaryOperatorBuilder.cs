using Bonsai.Properties;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that define a simple binary operator
    /// on paired elements of an observable sequence. This is an abstract class.
    /// </summary>
    public abstract class BinaryOperatorBuilder : SelectBuilder, IPropertyMappingBuilder, ISerializableElement, ICustomTypeDescriptor
    {
        static readonly MethodInfo GetEnumeratorMethod = typeof(IEnumerable).GetMethod("GetEnumerator");
        static readonly MethodInfo MoveNextMethod = typeof(IEnumerator).GetMethod("MoveNext");

        /// <summary>
        /// Gets or sets the value of the right hand operand which will be paired with elements
        /// of the observable sequence in case the sequence itself is not composed of paired elements.
        /// </summary>
        [Browsable(false)]
        public WorkflowProperty Operand { get; set; }

        object ISerializableElement.Element
        {
            get { return Operand; }
        }

        /// <summary>
        /// Gets the collection of property mappings assigned to this expression builder.
        /// Property mapping subscriptions are processed before evaluating other output generation
        /// expressions.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        [XmlArrayItem("PropertyMapping")]
        public PropertyMappingCollection PropertyMappings { get; } = new PropertyMappingCollection();

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

        private Expression ConvertAndBuildSelector(Expression left, Expression right)
        {
            if (left.Type != right.Type && left.Type.IsPrimitive && right.Type.IsPrimitive)
            {
                var comparison = CompareConversion(left.Type, right.Type, typeof(object));
                if (comparison < 0) left = Expression.Convert(left, right.Type);
                else right = Expression.Convert(right, left.Type);
            }

            return BuildSelector(left, right);
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
            var operand = Operand;
            Expression left, right;
            var expressionTypeDefinition = expression.Type.IsGenericType ? expression.Type.GetGenericTypeDefinition() : null;
            if (expressionTypeDefinition == typeof(Tuple<,>) ||
                expressionTypeDefinition == typeof(Tuple<,,>) ||
                expressionTypeDefinition == typeof(Tuple<,,,>) ||
                expressionTypeDefinition == typeof(Tuple<,,,,>) ||
                expressionTypeDefinition == typeof(Tuple<,,,,,>) ||
                expressionTypeDefinition == typeof(Tuple<,,,,,,>))
            {
                if (operand != null)
                {
                    throw new InvalidOperationException(Resources.Exception_BinaryOperationNotAllowed);
                }

                left = ExpressionHelper.MemberAccess(expression, "Item1");
                right = ExpressionHelper.MemberAccess(expression, "Item2");

                if (expressionTypeDefinition != typeof(Tuple<,>))
                {
                    left = ConvertAndBuildSelector(left, right);
                    right = ExpressionHelper.MemberAccess(expression, "Item3");
                    if (expressionTypeDefinition != typeof(Tuple<,,>))
                    {
                        left = ConvertAndBuildSelector(left, right);
                        right = ExpressionHelper.MemberAccess(expression, "Item4");
                        if (expressionTypeDefinition != typeof(Tuple<,,,>))
                        {
                            left = ConvertAndBuildSelector(left, right);
                            right = ExpressionHelper.MemberAccess(expression, "Item5");
                            if (expressionTypeDefinition != typeof(Tuple<,,,,>))
                            {
                                left = ConvertAndBuildSelector(left, right);
                                right = ExpressionHelper.MemberAccess(expression, "Item6");
                                if (expressionTypeDefinition != typeof(Tuple<,,,,,>))
                                {
                                    left = ConvertAndBuildSelector(left, right);
                                    right = ExpressionHelper.MemberAccess(expression, "Item7");
                                }
                            }
                        }
                    }
                }
            }
            else if (expression.Type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(expression.Type))
            {
                if (operand != null)
                {
                    throw new InvalidOperationException(Resources.Exception_BinaryOperationNotAllowed);
                }

                var genericEnumerable = Array.Find(expression.Type.GetInterfaces(), type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                var enumeratorMethod = genericEnumerable != null ? genericEnumerable.GetMethod("GetEnumerator") : GetEnumeratorMethod;
                var enumeratorCall = Expression.Call(expression, enumeratorMethod);
                var elementType = enumeratorCall.Type.IsGenericType ? enumeratorCall.Type.GetGenericArguments()[0] : typeof(object);
                var enumerator = Expression.Variable(enumeratorCall.Type);
                var accumulator = Expression.Variable(elementType);
                var loopLabel = Expression.Label(elementType);
                var moveNext = Expression.Call(enumerator, MoveNextMethod);
                var current = Expression.Property(enumerator, "Current");
                return Expression.Block(new[] { enumerator, accumulator },
                    Expression.Assign(enumerator, enumeratorCall),
                    Expression.IfThen(
                        Expression.Not(moveNext),
                        Expression.Throw(Expression.Constant(
                            new InvalidOperationException("The sequence must have at least one argument.")))),
                    Expression.Assign(accumulator, current),
                    Expression.Loop(
                        Expression.IfThenElse(
                            moveNext,
                            Expression.Assign(accumulator, ConvertAndBuildSelector(accumulator, current)),
                            Expression.Break(loopLabel, accumulator)),
                        loopLabel),
                    accumulator);
            }
            else
            {
                if (operand == null)
                {
                    if (expression.Type.IsInterface ||
                        expression.Type.IsClass && expression.Type != typeof(string) &&
                        expression.Type.GetConstructor(Type.EmptyTypes) == null)
                    {
                        throw new InvalidOperationException(string.Format(Resources.Exception_UnsupportedMinArgumentCount, 2));
                    }

                    var propertyType = GetWorkflowPropertyType(expression.Type);
                    Operand = operand = (WorkflowProperty)Activator.CreateInstance(propertyType);
                }

                left = expression;
                var operandExpression = Expression.Constant(operand);
                right = Expression.Property(operandExpression, "Value");
            }

            return ConvertAndBuildSelector(left, right);
        }

        #region ICustomTypeDescriptor Members

        static readonly Attribute[] ValueAttributes = new[]
        {
            new DescriptionAttribute("The value of the right hand operand in the binary operator.")
        };

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(GetType());
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(GetType());
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return null;
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(GetType());
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return null;
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            var operand = Operand;
            return operand != null ? new WorkflowPropertyDescriptor("Value", ValueAttributes, operand) : null;
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(GetType(), editorBaseType);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return EventDescriptorCollection.Empty;
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return EventDescriptorCollection.Empty;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(ValueAttributes);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            var defaultProperty = ((ICustomTypeDescriptor)this).GetDefaultProperty();
            if (defaultProperty != null)
            {
                return new PropertyDescriptorCollection(new[] { defaultProperty });
            }
            else return PropertyDescriptorCollection.Empty;
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            return (object)Operand ?? this;
        }

        #endregion
    }
}
