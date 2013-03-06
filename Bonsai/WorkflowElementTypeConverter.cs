using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;

namespace Bonsai
{
    public static class WorkflowElementTypeConverter
    {
        static bool MatchType(Type type, WorkflowElementType elementType)
        {
            if (elementType == WorkflowElementType.Source) return MatchGenericType(type, typeof(Source<>));
            if (elementType == WorkflowElementType.Condition) return type.GetCustomAttributes(typeof(ConditionAttribute), true).Length > 0;
            if (elementType == WorkflowElementType.Sink) return type.IsSubclassOf(typeof(DynamicSink)) || type.GetCustomAttributes(typeof(SinkAttribute), true).Length > 0;
            if (elementType == WorkflowElementType.Combinator) return type.IsSubclassOf(typeof(ExpressionBuilder));
            if (elementType == WorkflowElementType.Transform) return type.GetCustomAttributes(typeof(TransformAttribute), true).Length > 0;
            return false;
        }

        static bool MatchIgnoredTypes(Type type)
        {
            return type == typeof(SourceBuilder) ||
                   type == typeof(SelectBuilder) ||
                   type == typeof(WhereBuilder) ||
                   type == typeof(DoBuilder) ||
                   type == typeof(PublishBuilder) ||
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

        public static IEnumerable<WorkflowElementType> FromType(Type type)
        {
            if (MatchIgnoredTypes(type)) yield break;
            if (MatchType(type, WorkflowElementType.Source)) yield return WorkflowElementType.Source;
            if (MatchType(type, WorkflowElementType.Condition)) yield return WorkflowElementType.Condition;
            if (MatchType(type, WorkflowElementType.Transform)) yield return WorkflowElementType.Transform;
            if (MatchType(type, WorkflowElementType.Sink)) yield return WorkflowElementType.Sink;
            if (MatchType(type, WorkflowElementType.Combinator)) yield return WorkflowElementType.Combinator;
        }
    }
}
