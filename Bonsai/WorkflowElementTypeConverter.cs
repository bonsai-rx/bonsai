using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using System.ComponentModel;

namespace Bonsai
{
    public static class WorkflowElementTypeConverter
    {
        static bool MatchType(Type type, ElementCategory elementType)
        {
            if (elementType == ElementCategory.Source) return MatchGenericType(type, typeof(Source<>));
            if (elementType == ElementCategory.Condition) return type.GetCustomAttributes(typeof(ConditionAttribute), true).Length > 0;
            if (elementType == ElementCategory.Sink) return type.GetCustomAttributes(typeof(SinkAttribute), true).Length > 0;
            if (elementType == ElementCategory.Combinator) return type.IsSubclassOf(typeof(ExpressionBuilder));
            if (elementType == ElementCategory.Transform) return type.GetCustomAttributes(typeof(TransformAttribute), true).Length > 0;
            return false;
        }

        static bool MatchIgnoredTypes(Type type)
        {
            return type == typeof(SourceBuilder) ||
                   type == typeof(SelectBuilder) ||
                   type == typeof(WhereBuilder) ||
                   type == typeof(DoBuilder) ||
                   type == typeof(CombinatorBuilder) ||
                   type == typeof(InspectBuilder);
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
                MatchAttributeType(type, typeof(BinaryCombinatorAttribute)))
            {
                var attributes = TypeDescriptor.GetAttributes(type);
                var elementCategoryAttribute = (WorkflowElementCategoryAttribute)attributes[typeof(WorkflowElementCategoryAttribute)];
                yield return elementCategoryAttribute.Category;
            }
            else
            {
                if (MatchType(type, ElementCategory.Source)) yield return ElementCategory.Source;
                if (MatchAttributeType(type, typeof(ConditionAttribute))) yield return ElementCategory.Condition;
                if (MatchAttributeType(type, typeof(TransformAttribute))) yield return ElementCategory.Transform;
                if (MatchAttributeType(type, typeof(SinkAttribute))) yield return ElementCategory.Sink;
            }
        }
    }
}
