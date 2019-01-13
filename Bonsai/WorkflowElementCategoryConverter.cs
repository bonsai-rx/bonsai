using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using System.ComponentModel;

namespace Bonsai
{
    static class WorkflowElementCategoryConverter
    {
        static bool MatchIgnoredTypes(Type type)
        {
            return type == typeof(SourceBuilder) ||
                   type == typeof(CombinatorBuilder) ||
                   type == typeof(InspectBuilder) ||
                   type == typeof(ExternalizedProperty) ||
                   type == typeof(DisableBuilder);
        }

        static bool MatchGenericType(Type type, Type genericType)
        {
            if (!genericType.IsGenericType)
            {
                throw new ArgumentException("Trying to match against a non-generic type.", "genericType");
            }

            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        static bool MatchAttributeType(Type type, Type attributeType)
        {
            return type.IsDefined(attributeType, true);
        }

        public static IEnumerable<ElementCategory> FromType(Type type)
        {
            if (MatchIgnoredTypes(type)) yield break;

            if (type.IsSubclassOf(typeof(ExpressionBuilder)) ||
                MatchAttributeType(type, typeof(CombinatorAttribute)) ||
                MatchAttributeType(type, typeof(SourceAttribute)))
            {
                if (type.IsSubclassOf(typeof(WorkflowExpressionBuilder)))
                {
                    yield return ElementCategory.Nested;
                }

                var attributes = TypeDescriptor.GetAttributes(type);
                var elementCategoryAttribute = (WorkflowElementCategoryAttribute)attributes[typeof(WorkflowElementCategoryAttribute)];
                yield return elementCategoryAttribute.Category;
            }
        }
    }
}
