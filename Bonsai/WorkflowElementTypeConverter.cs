﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using System.ComponentModel;

namespace Bonsai
{
    public static class WorkflowElementTypeConverter
    {
        static bool MatchIgnoredTypes(Type type)
        {
            return type == typeof(SourceBuilder) ||
                   type == typeof(ConditionBuilder) ||
                   type == typeof(SelectBuilder) ||
                   type == typeof(WhereBuilder) ||
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
                MatchAttributeType(type, typeof(CombinatorAttribute)))
            {
                var attributes = TypeDescriptor.GetAttributes(type);
                var elementCategoryAttribute = (WorkflowElementCategoryAttribute)attributes[typeof(WorkflowElementCategoryAttribute)];
                yield return elementCategoryAttribute.Category;
                if (type.IsDefined(typeof(ConditionAttribute), true))
                {
                    yield return ElementCategory.Condition;
                }
            }
            else
            {
                if (MatchGenericType(type, typeof(Source<>))) yield return ElementCategory.Source;
                if (MatchAttributeType(type, typeof(PredicateAttribute))) yield return ElementCategory.Condition;
                if (MatchAttributeType(type, typeof(SelectorAttribute))) yield return ElementCategory.Transform;
            }
        }
    }
}
