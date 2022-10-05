using System;
using System.Collections.Generic;
using Bonsai.Expressions;
using System.ComponentModel;

namespace Bonsai
{
    static class WorkflowElementCategoryConverter
    {
        static bool MatchIgnoredTypes(Type type)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            return type == typeof(SourceBuilder) ||
#pragma warning restore CS0612 // Type or member is obsolete
                   type == typeof(CombinatorBuilder) ||
                   type == typeof(InspectBuilder) ||
                   type == typeof(ExternalizedProperty) ||
                   type == typeof(DisableBuilder);
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
#pragma warning disable CS0612 // Type or member is obsolete
                MatchAttributeType(type, typeof(SourceAttribute)))
#pragma warning restore CS0612 // Type or member is obsolete
            {
                if (type.IsSubclassOf(typeof(WorkflowExpressionBuilder)))
                {
                    yield return ElementCategory.Nested;
                }

                if (type.IsSubclassOf(typeof(SubjectExpressionBuilder)) ||
                    type == typeof(WorkflowInputBuilder))
                {
                    yield return ~ElementCategory.Combinator;
                }

                var attributes = TypeDescriptor.GetAttributes(type);
                var elementCategoryAttribute = (WorkflowElementCategoryAttribute)attributes[typeof(WorkflowElementCategoryAttribute)];
                yield return elementCategoryAttribute.Category;
            }
        }
    }
}
