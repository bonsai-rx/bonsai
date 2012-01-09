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

        public static bool MatchGenericType(Type type, Type genericType)
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
            if (MatchGenericType(type, typeof(Source<>))) return LoadableElementType.Source;
            if (MatchGenericType(type, typeof(Filter<>))) return LoadableElementType.Filter;
            if (MatchGenericType(type, typeof(Projection<,>))) return LoadableElementType.Projection;
            if (MatchGenericType(type, typeof(Sink<>))) return LoadableElementType.Sink;
            throw new ArgumentException("Invalid loadable element type.");
        }
    }
}
