using System;
using System.ComponentModel;
using System.Text;
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
            string description;
            if (component is WorkflowExpressionBuilder workflowExpressionBuilder)
            {
                description = workflowExpressionBuilder.Description;
                if (!string.IsNullOrEmpty(description)) return description;
            }

            var attributes = TypeDescriptor.GetAttributes(component);
            var descriptionAttribute = (DescriptionAttribute)attributes[typeof(DescriptionAttribute)];
            description = descriptionAttribute.Description;

            if (attributes[typeof(ObsoleteAttribute)] is ObsoleteAttribute obsoleteAttribute)
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.Append("This operator is obsolete");
                if (!string.IsNullOrEmpty(obsoleteAttribute.Message))
                    messageBuilder.AppendFormat(": {0}", obsoleteAttribute.Message);
                messageBuilder.AppendLine();
                messageBuilder.AppendLine();
                messageBuilder.Append(description);
                description = messageBuilder.ToString();
            }

            return description;
        }
    }
}
