using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Reflection;

namespace Bonsai.Expressions
{
    [XmlInclude(typeof(CombinatorExpressionBuilder))]
    [XmlInclude(typeof(WorkflowExpressionBuilder))]
    [XmlInclude(typeof(WorkflowInputBuilder))]
    [XmlInclude(typeof(WorkflowOutputBuilder))]
    [XmlInclude(typeof(SourceBuilder))]
    [XmlInclude(typeof(SelectBuilder))]
    [XmlInclude(typeof(WhereBuilder))]
    [XmlInclude(typeof(DoBuilder))]
    [XmlInclude(typeof(NullSinkBuilder))]
    [XmlInclude(typeof(UnitBuilder))]
    [XmlInclude(typeof(MemberSelectorBuilder))]
    [XmlInclude(typeof(DistinctUntilChangedBuilder))]
    [XmlInclude(typeof(TimestampBuilder))]
    [XmlInclude(typeof(CombineTimestampBuilder))]
    [XmlInclude(typeof(RepeatBuilder))]
    [XmlInclude(typeof(RepeatCountBuilder))]
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
    [XmlInclude(typeof(MergeBuilder))]
    [XmlInclude(typeof(SelectManyBuilder))]
    [XmlInclude(typeof(TimeSpanWindowBuilder))]
    [XmlInclude(typeof(ElementCountWindowBuilder))]
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
            if (selectBuilder != null) return selectBuilder.Transform.GetType();

            var whereBuilder = builder as WhereBuilder;
            if (whereBuilder != null) return whereBuilder.Condition.GetType();

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

            if (elementType == WorkflowElementType.Source) return new SourceBuilder { Source = element };
            if (elementType == WorkflowElementType.Condition) return new WhereBuilder { Condition = element };
            if (elementType == WorkflowElementType.Transform) return new SelectBuilder { Transform = element };
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

        internal static Expression BuildProcessExpression(Expression parameter, object processor, MethodInfo processMethod)
        {
            if (processMethod.IsGenericMethodDefinition)
            {
                processMethod = processMethod.MakeGenericMethod(parameter.Type);
            }

            var processorExpression = Expression.Constant(processor);
            var parameterType = processMethod.GetParameters()[0].ParameterType;
            var processParameter = (Expression)parameter;
            if (parameter.Type != parameterType)
            {
                processParameter = Expression.Convert(processParameter, parameterType);
            }

            return Expression.Call(processorExpression, processMethod, processParameter);
        }
    }
}
