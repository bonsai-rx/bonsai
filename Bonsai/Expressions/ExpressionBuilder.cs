using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlInclude(typeof(CombinatorExpressionBuilder))]
    [XmlInclude(typeof(WorkflowExpressionBuilder))]
    [XmlInclude(typeof(WorkflowInputBuilder))]
    [XmlInclude(typeof(SourceBuilder))]
    [XmlInclude(typeof(SelectBuilder))]
    [XmlInclude(typeof(WhereBuilder))]
    [XmlInclude(typeof(DoBuilder))]
    [XmlInclude(typeof(MemberSelectorBuilder))]
    [XmlInclude(typeof(DistinctUntilChangedBuilder))]
    [XmlInclude(typeof(TimestampBuilder))]
    [XmlInclude(typeof(RepeatBuilder))]
    [XmlInclude(typeof(TimeIntervalBuilder))]
    [XmlInclude(typeof(ThrottleBuilder))]
    [XmlInclude(typeof(SampleBuilder))]
    [XmlInclude(typeof(SampleIntervalBuilder))]
    [XmlInclude(typeof(GateBuilder))]
    [XmlInclude(typeof(GateIntervalBuilder))]
    [XmlInclude(typeof(TimedGateBuilder))]
    [XmlInclude(typeof(SkipBuilder))]
    [XmlInclude(typeof(SkipLastBuilder))]
    [XmlInclude(typeof(SkipUntilBuilder))]
    [XmlInclude(typeof(SubscribeWhenBuilder))]
    [XmlInclude(typeof(TakeBuilder))]
    [XmlInclude(typeof(TakeLastBuilder))]
    [XmlInclude(typeof(TakeUntilBuilder))]
    [XmlInclude(typeof(CombineLatestBuilder))]
    [XmlInclude(typeof(ConcatBuilder))]
    [XmlInclude(typeof(DelayBuilder))]
    [XmlInclude(typeof(ZipBuilder))]
    [XmlInclude(typeof(AmbBuilder))]
    [XmlType("Expression", Namespace = Constants.XmlNamespace)]
    [TypeConverter("Bonsai.Design.ExpressionBuilderTypeConverter, Bonsai.Design")]
    public abstract class ExpressionBuilder
    {
        public abstract Expression Build();

        public static Type GetWorkflowElementType(ExpressionBuilder builder)
        {
            var sourceBuilder = builder as SourceBuilder;
            if (sourceBuilder != null) return sourceBuilder.Source.GetType();

            var selectBuilder = builder as SelectBuilder;
            if (selectBuilder != null) return selectBuilder.Projection.GetType();

            var whereBuilder = builder as WhereBuilder;
            if (whereBuilder != null) return whereBuilder.Filter.GetType();

            var doBuilder = builder as DoBuilder;
            if (doBuilder != null) return doBuilder.Sink.GetType();

            return builder.GetType();
        }

        public static ExpressionBuilder FromLoadableElement(LoadableElement element, WorkflowElementType elementType)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (elementType == WorkflowElementType.Source) return new SourceBuilder { Source = (Source)element };
            if (elementType == WorkflowElementType.Filter) return new WhereBuilder { Filter = element };
            if (elementType == WorkflowElementType.Projection) return new SelectBuilder { Projection = element };
            if (elementType == WorkflowElementType.Sink) return new DoBuilder { Sink = element };
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

        protected static Type GetSinkGenericArgument(LoadableElement sink)
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
                    return type.GetGenericArguments()[0];
                }

                type = type.BaseType;
            }

            return null;
        }
    }
}
