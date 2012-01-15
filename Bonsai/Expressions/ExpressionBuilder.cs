using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("Expression")]
    [XmlInclude(typeof(CombinatorBuilder))]
    [XmlInclude(typeof(SourceBuilder))]
    [XmlInclude(typeof(SelectBuilder))]
    [XmlInclude(typeof(WhereBuilder))]
    [XmlInclude(typeof(DoBuilder))]
    [XmlInclude(typeof(TimestampBuilder))]
    [XmlInclude(typeof(SampleBuilder))]
    [XmlInclude(typeof(SampleIntervalBuilder))]
    [XmlInclude(typeof(SkipUntilBuilder))]
    [XmlInclude(typeof(TakeUntilBuilder))]
    [XmlInclude(typeof(CombineLatestBuilder))]
    [XmlInclude(typeof(ConcatBuilder))]
    [XmlInclude(typeof(ZipBuilder))]
    [XmlInclude(typeof(AmbBuilder))]
    [TypeConverter("Bonsai.Design.ExpressionBuilderTypeConverter, Bonsai.Design")]
    public abstract class ExpressionBuilder
    {
        public abstract Expression Build();

        public static ExpressionBuilder FromLoadableElement(LoadableElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            var elementType = LoadableElementType.FromType(element.GetType());
            if (elementType == LoadableElementType.Source) return new SourceBuilder { Source = (Source)element };
            if (elementType == LoadableElementType.Filter) return new WhereBuilder { Filter = element };
            if (elementType == LoadableElementType.Projection) return new SelectBuilder { Projection = element };
            if (elementType == LoadableElementType.Sink) return new DoBuilder { Sink = element };
            throw new InvalidOperationException("Invalid loadable element type.");
        }

        protected static Type GetObservableType(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return source.GetType()
                         .FindInterfaces((t, m) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IObservable<>), null)
                         .First()
                         .GetGenericArguments()[0];
        }

        protected static Type GetFilterGenericArgument(LoadableElement filter)
        {
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }

            var type = filter.GetType();
            while (type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Filter<>))
                {
                    return type.GetGenericArguments()[0];
                }

                type = type.BaseType;
            }

            return null;
        }

        protected static Type[] GetSinkGenericArguments(LoadableElement sink)
        {
            if (sink == null)
            {
                throw new ArgumentNullException("sink");
            }

            var type = sink.GetType();
            while (type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Sink<>))
                {
                    return type.GetGenericArguments();
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}
