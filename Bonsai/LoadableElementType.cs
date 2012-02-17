using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public class LoadableElementType
    {
        public static readonly LoadableElementType Source = new LoadableElementType("Source");
        public static readonly LoadableElementType Filter = new LoadableElementType("Filter");
        public static readonly LoadableElementType Projection = new LoadableElementType("Projection");
        public static readonly LoadableElementType Sink = new LoadableElementType("Sink");
        public static readonly LoadableElementType Combinator = new LoadableElementType("Combinator");

        private LoadableElementType(string text)
        {
            Text = text;
        }

        private string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }

        public static bool MatchType(Type type, LoadableElementType elementType)
        {
            if (elementType == LoadableElementType.Source) return MatchGenericType(type, typeof(Source<>));
            if (elementType == LoadableElementType.Filter) return MatchGenericType(type, typeof(Filter<>));
            if (elementType == LoadableElementType.Sink) return type.IsSubclassOf(typeof(DynamicSink)) || MatchGenericType(type, typeof(Sink<>));
            if (elementType == LoadableElementType.Combinator) return MatchGenericType(type, typeof(Combinator<,>));
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
            if (MatchType(type, LoadableElementType.Combinator)) return LoadableElementType.Combinator;
            return null;
        }
    }
}
