using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("Select", Namespace = Constants.XmlNamespace)]
    public class SelectBuilder : CombinatorExpressionBuilder
    {
        static readonly ConstructorInfo runtimeExceptionConstructor = typeof(WorkflowRuntimeException).GetConstructor(new[] { typeof(string), typeof(ExpressionBuilder), typeof(Exception) });
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public LoadableElement Transform { get; set; }

        public override Expression Build()
        {
            var transformType = Transform.GetType();
            var transformAttributes = transformType.GetCustomAttributes(typeof(TransformAttribute), true);
            var methodName = ((TransformAttribute)transformAttributes.Single()).MethodName;
            var processMethod = transformType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                                   .Single(m => m.Name == methodName && m.GetParameters().Length == 1);
            var parameter = Expression.Parameter(Source.Type.GetGenericArguments()[0]);
            var process = BuildProcessExpression(parameter, Transform, processMethod);

            var exception = Expression.Parameter(typeof(Exception));
            var exceptionText = Expression.Property(exception, "Message");
            var runtimeException = Expression.New(runtimeExceptionConstructor, exceptionText, Expression.Constant(this), exception);
            var selector = Expression.TryCatch(process, Expression.Catch(exception, Expression.Throw(runtimeException, process.Type)));
            return Expression.Call(selectMethod.MakeGenericMethod(parameter.Type, process.Type), Source, Expression.Lambda(selector, parameter));
        }
    }
}
