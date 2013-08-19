using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [WorkflowElementCategory(ElementCategory.Sink)]
    [XmlType("Do", Namespace = Constants.XmlNamespace)]
    public class DoBuilder : CombinatorExpressionBuilder
    {
        static readonly ConstructorInfo runtimeExceptionConstructor = typeof(WorkflowRuntimeException).GetConstructor(new[] { typeof(string), typeof(ExpressionBuilder), typeof(Exception) });
        static readonly MethodInfo doMethod = typeof(Observable).GetMethods()
                                                                .Single(m => m.Name == "Do" &&
                                                                        m.GetParameters().Length == 2 &&
                                                                        m.GetParameters()[1].ParameterType.IsGenericType &&
                                                                        m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>));

        public LoadableElement Sink { get; set; }

        public override Expression Build()
        {
            var sinkType = Sink.GetType();
            var sinkExpression = Expression.Constant(Sink);
            var sinkAttributes = sinkType.GetCustomAttributes(typeof(SinkAttribute), true);
            var methodName = ((SinkAttribute)sinkAttributes.Single()).MethodName;
            var parameter = Expression.Parameter(Source.Type.GetGenericArguments()[0]);
            var processMethod = sinkType.GetMethod(methodName);
            var process = BuildCall(sinkExpression, processMethod, parameter);
            return Expression.Call(doMethod.MakeGenericMethod(parameter.Type), Source, Expression.Lambda(process, parameter));
        }
    }
}
