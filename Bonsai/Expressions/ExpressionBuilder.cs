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
    [XmlInclude(typeof(WindowWorkflowBuilder))]
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

        internal static Type[] GetMethodBindings(MethodInfo methodInfo, params Type[] parameters)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }

            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            var methodParameters = methodInfo.GetParameters();
            var methodGenericArguments = methodInfo.GetGenericArguments();

            // The binding candidates are the distinct results from matching parameters with input
            // Matches for the same generic parameter position should be identical
            var bindingCandidates = (from bindings in methodParameters.Zip(parameters, (methodParameter, parameter) => GetParameterBindings(methodParameter.ParameterType, parameter))
                                     from binding in bindings
                                     group binding by binding.Item2 into matches
                                     orderby matches.Key ascending
                                     select matches.Distinct().Single().Item1)
                                     .ToArray();

            return methodGenericArguments.Zip(bindingCandidates, (argument, match) => match).Concat(methodGenericArguments.Skip(bindingCandidates.Length)).ToArray();
        }

        internal static IEnumerable<Tuple<Type, int>> GetParameterBindings(Type parameterType, Type inputType)
        {
            // If parameter is a generic parameter, just bind it to the input type
            if (parameterType.IsGenericParameter)
            {
                return Enumerable.Repeat(Tuple.Create(inputType, parameterType.GenericParameterPosition), 1);
            }
            // If parameter contains generic parameters, we may have possible bindings
            else if (parameterType.ContainsGenericParameters)
            {
                // Check if we have a straight type match
                var bindings = MatchTypeBindings(parameterType, inputType).ToArray();
                if (bindings.Length > 0) return bindings;

                // Direct match didn't produce any bindings, so we need to check inheritance chain
                Type currentType = inputType;
                while (currentType != typeof(object))
                {
                    currentType = currentType.BaseType;
                    bindings = MatchTypeBindings(parameterType, currentType).ToArray();
                    if (bindings.Length > 0) return bindings;
                }

                // Inheritance chain match didn't produce any bindings, so we need to check interface set
                var interfaces = inputType.GetInterfaces();
                foreach (var interfaceType in interfaces)
                {
                    bindings = MatchTypeBindings(parameterType, interfaceType).ToArray();
                    if (bindings.Length > 0) return bindings;
                }
            }

            // If parameter does not contain generic parameters, there's nothing to bind to (check for error?)
            return Enumerable.Empty<Tuple<Type, int>>();
        }

        internal static IEnumerable<Tuple<Type, int>> MatchTypeBindings(Type parameterType, Type inputType)
        {
            // Match bindings can only be obtained if both types are generic types
            if (parameterType.IsGenericType && inputType.IsGenericType)
            {
                var parameterTypeDefinition = parameterType.GetGenericTypeDefinition();
                var inputTypeDefinition = parameterType.GetGenericTypeDefinition();
                // Match bindings can only be obtained if both types share the same type definition
                if (parameterTypeDefinition == inputTypeDefinition)
                {
                    var parameterGenericArguments = parameterType.GetGenericArguments();
                    var inputGenericArguments = inputType.GetGenericArguments();
                    return parameterGenericArguments
                        .Zip(inputGenericArguments, (parameter, input) => GetParameterBindings(parameter, input))
                        .SelectMany(xs => xs);
                }
            }

            return Enumerable.Empty<Tuple<Type, int>>();
        }

        internal static Expression BuildProcessExpression(Expression parameter, object processor, MethodInfo processMethod)
        {
            if (processMethod.IsGenericMethodDefinition)
            {
                var typeArguments = GetMethodBindings(processMethod, parameter.Type);
                processMethod = processMethod.MakeGenericMethod(typeArguments.ToArray());
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
