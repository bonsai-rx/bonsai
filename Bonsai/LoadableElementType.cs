using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public class LoadableElementType
    {
        public static readonly LoadableElementType Source = new LoadableElementType();
        public static readonly LoadableElementType Filter = new LoadableElementType();
        public static readonly LoadableElementType Projection = new LoadableElementType();
        public static readonly LoadableElementType Sink = new LoadableElementType();

        private LoadableElementType()
        {
        }

        public static bool MatchType(Type type, LoadableElementType elementType)
        {
            if (elementType == LoadableElementType.Source) return MatchGenericType(type, typeof(Source<>));
            if (elementType == LoadableElementType.Filter) return MatchGenericType(type, typeof(Filter<>));
            if (elementType == LoadableElementType.Sink) return MatchGenericType(type, typeof(Sink<>));
            if (elementType == LoadableElementType.Projection)
            {
                return MatchGenericType(type, typeof(Projection<,>)) ||
                       MatchGenericType(type, typeof(Projection<,,>));
            }

            return false;
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

        public static LoadableElementType FromType(Type type)
        {
            if (MatchType(type, LoadableElementType.Source)) return LoadableElementType.Source;
            if (MatchType(type, LoadableElementType.Filter)) return LoadableElementType.Filter;
            if (MatchType(type, LoadableElementType.Projection)) return LoadableElementType.Projection;
            if (MatchType(type, LoadableElementType.Sink)) return LoadableElementType.Sink;
            return null;
        }
    }
}
