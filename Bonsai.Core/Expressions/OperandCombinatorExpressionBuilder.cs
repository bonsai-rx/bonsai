using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders combining a single observable sequence with
    /// an auxiliary operand value whose type is inferred from the sequence. This is an abstract class.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    [WorkflowElementCategory(ElementCategory.Combinator)]
    public abstract class OperandCombinatorExpressionBuilder : BinaryOperatorBuilder
    {
        internal OperandCombinatorExpressionBuilder()
        {
        }

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            return BuildSelector(source);
        }

        /// <inheritdoc/>
        protected override Expression BuildSelector(Expression expression)
        {
            var operand = Operand;
            var source = expression;
            var parameterType = source.Type.GetGenericArguments()[0];
            if (operand is null)
            {
                if (parameterType.IsInterface ||
                    parameterType.IsClass && parameterType != typeof(string) &&
                    parameterType.GetConstructor(Type.EmptyTypes) == null)
                {
                    throw new InvalidOperationException(
                        $"{parameterType} cannot be used as operand because it does not have a parameterless constructor.");
                }

                var propertyType = GetWorkflowPropertyType(parameterType);
                Operand = operand = (WorkflowProperty)Activator.CreateInstance(propertyType);
            }

            var operandExpression = ExpressionHelper.Property(
                Expression.Constant(operand),
                "Value");
            return BuildSelector(source, operandExpression);
        }
    }
}
