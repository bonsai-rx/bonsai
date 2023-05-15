using System.ComponentModel;
using Bonsai.Expressions;

namespace Bonsai.Editor.GraphModel
{
    static class ElementHelper
    {
        public static string GetElementName(object component)
        {
            var name = ExpressionBuilder.GetElementDisplayName(component);
            if (component is ExternalizedProperty workflowProperty &&
                !string.IsNullOrWhiteSpace(workflowProperty.Name) &&
                workflowProperty.Name != workflowProperty.MemberName)
            {
                return name + " (" + workflowProperty.MemberName + ")";
            }

            var componentType = component.GetType();
            if (component is BinaryOperatorBuilder binaryOperator && binaryOperator.Operand != null)
            {
                var operandType = binaryOperator.Operand.GetType();
                if (operandType.IsGenericType) operandType = operandType.GetGenericArguments()[0];
                return name + " (" + ExpressionBuilder.GetElementDisplayName(operandType) + ")";
            }
            else if (component is SubscribeSubject subscribeSubject && componentType.IsGenericType)
            {
                componentType = componentType.GetGenericArguments()[0];
                if (string.IsNullOrWhiteSpace(subscribeSubject.Name))
                {
                    name = name.Substring(0, name.IndexOf("`"));
                }
                return name + " (" + ExpressionBuilder.GetElementDisplayName(componentType) + ")";
            }
            else
            {
                if (component is INamedElement namedExpressionBuilder && !string.IsNullOrWhiteSpace(namedExpressionBuilder.Name))
                {
                    name += " (" + ExpressionBuilder.GetElementDisplayName(componentType) + ")";
                }

                return name;
            }
        }

        public static string GetElementDescription(object component)
        {
            if (component is WorkflowExpressionBuilder workflowExpressionBuilder)
            {
                var description = workflowExpressionBuilder.Description;
                if (!string.IsNullOrEmpty(description)) return description;
            }

            var descriptionAttribute = (DescriptionAttribute)TypeDescriptor.GetAttributes(component)[typeof(DescriptionAttribute)];
            return descriptionAttribute.Description;
        }
    }
}
