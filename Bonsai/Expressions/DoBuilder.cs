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
            var sink = Sink;
            var dynamicSink = sink as DynamicSink;
            var observableType = Source.Type.GetGenericArguments()[0];
            if (dynamicSink != null)
            {
                var createMethod = dynamicSink.GetType().GetMethod("Create");
                createMethod = createMethod.MakeGenericMethod(observableType);
                sink = (LoadableElement)createMethod.Invoke(dynamicSink, null);
            }

            var parameter = Expression.Parameter(observableType);
            var processMethod = sink.GetType().GetMethod("Process");
            var process = BuildProcessExpression(parameter, sink, processMethod);

            var exception = Expression.Parameter(typeof(Exception));
            var exceptionText = Expression.Property(exception, "Message");
            var runtimeException = Expression.New(runtimeExceptionConstructor, exceptionText, Expression.Constant(this), exception);
            var action = Expression.TryCatch(process, Expression.Catch(exception, Expression.Throw(runtimeException, process.Type)));
            return Expression.Call(doMethod.MakeGenericMethod(observableType), Source, Expression.Lambda(action, parameter));
        }
    }
}
