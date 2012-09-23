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
            if (elementType == WorkflowElementType.Filter) return MatchGenericType(type, typeof(Filter<>));
            if (elementType == WorkflowElementType.Sink) return type.IsSubclassOf(typeof(DynamicSink)) || MatchGenericType(type, typeof(Sink<>));
            if (elementType == WorkflowElementType.Combinator) return type.IsSubclassOf(typeof(ExpressionBuilder));
            if (elementType == WorkflowElementType.Projection)
            {
                return MatchGenericType(type, typeof(Projection<,>)) ||
                       MatchGenericType(type, typeof(Projection<,,>));
            }

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
            if (MatchType(type, WorkflowElementType.Filter)) yield return WorkflowElementType.Filter;
            if (MatchType(type, WorkflowElementType.Projection)) yield return WorkflowElementType.Projection;
            if (MatchType(type, WorkflowElementType.Sink)) yield return WorkflowElementType.Sink;
            if (MatchType(type, WorkflowElementType.Combinator)) yield return WorkflowElementType.Combinator;
        }
    }
}
