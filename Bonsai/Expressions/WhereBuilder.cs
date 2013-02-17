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
    [XmlType("Where", Namespace = Constants.XmlNamespace)]
    public class WhereBuilder : CombinatorExpressionBuilder
    {
        static readonly ConstructorInfo runtimeExceptionConstructor = typeof(WorkflowRuntimeException).GetConstructor(new[] { typeof(string), typeof(ExpressionBuilder), typeof(Exception) });
        static readonly MethodInfo whereMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Where" &&
                                                                           m.GetParameters().Length == 2 &&
                                                                           m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public LoadableElement Condition { get; set; }

        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var processMethod = Condition.GetType().GetMethod("Process");
            var parameter = Expression.Parameter(observableType);
            var process = BuildProcessExpression(parameter, Condition, processMethod);

            var exception = Expression.Parameter(typeof(Exception));
            var exceptionText = Expression.Property(exception, "Message");
            var runtimeException = Expression.New(runtimeExceptionConstructor, exceptionText, Expression.Constant(this), exception);
            var predicate = Expression.TryCatch(process, Expression.Catch(exception, Expression.Throw(runtimeException, process.Type)));
            return Expression.Call(whereMethod.MakeGenericMethod(observableType), Source, Expression.Lambda(predicate, parameter));
        }
    }
}
