using System;
using System.Collections.Generic;
using Bonsai.Expressions;
using System.Reflection;

namespace Bonsai
{
    static class WorkflowElementCategoryConverter
    {
        static bool MatchIgnoredTypes(Type type)
        {
            var typeName = type.AssemblyQualifiedName;
#pragma warning disable CS0612 // Type or member is obsolete
            return typeName == typeof(SourceBuilder).AssemblyQualifiedName ||
#pragma warning restore CS0612 // Type or member is obsolete
                   typeName == typeof(CombinatorBuilder).AssemblyQualifiedName ||
                   typeName == typeof(InspectBuilder).AssemblyQualifiedName ||
                   typeName == typeof(ExternalizedProperty).AssemblyQualifiedName ||
                   typeName == typeof(DisableBuilder).AssemblyQualifiedName;
        }

        static bool MatchAttributeType(CustomAttributeData[] customAttributes, Type attributeType)
        {
            return customAttributes.IsDefined(attributeType);
        }

        public static IEnumerable<ElementCategory> FromType(Type type, CustomAttributeData[] customAttributes)
        {
            if (MatchIgnoredTypes(type)) yield break;

            if (type.IsMatchSubclassOf(typeof(ExpressionBuilder)) ||
                MatchAttributeType(customAttributes, typeof(CombinatorAttribute)) ||
#pragma warning disable CS0612 // Type or member is obsolete
                MatchAttributeType(customAttributes, typeof(SourceAttribute)))
#pragma warning restore CS0612 // Type or member is obsolete
            {
                if (type.IsMatchSubclassOf(typeof(WorkflowExpressionBuilder)))
                {
                    yield return ElementCategory.Nested;
                }

                if (type.IsMatchSubclassOf(typeof(SubjectExpressionBuilder)) ||
                    type.AssemblyQualifiedName == typeof(WorkflowInputBuilder).AssemblyQualifiedName)
                {
                    yield return ~ElementCategory.Combinator;
                }

                var elementCategoryAttribute = customAttributes.GetCustomAttributeData(typeof(WorkflowElementCategoryAttribute));
                yield return elementCategoryAttribute != null
                    ? (ElementCategory)elementCategoryAttribute.GetConstructorArgument()
                    : WorkflowElementCategoryAttribute.Default.Category;
            }
        }
    }
}
