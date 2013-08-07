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
    [WorkflowElementCategory(ElementCategory.Condition)]
    [XmlType("Condition", Namespace = Constants.XmlNamespace)]
    public class ConditionBuilder : CombinatorExpressionBuilder
    {
        static readonly ConstructorInfo runtimeExceptionConstructor = typeof(WorkflowRuntimeException).GetConstructor(new[] { typeof(string), typeof(ExpressionBuilder), typeof(Exception) });
        static readonly MethodInfo whereMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Where" &&
                                                                           m.GetParameters().Length == 2 &&
                                                                           m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public LoadableElement Condition { get; set; }

        [Description("The inner property on which to apply the condition.")]
        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Selector { get; set; }

        public override Expression Build()
        {
            var conditionType = Condition.GetType();
            var conditionExpression = Expression.Constant(Condition);
            var conditionAttributes = conditionType.GetCustomAttributes(typeof(ConditionAttribute), true);
            var methodName = ((ConditionAttribute)conditionAttributes.Single()).MethodName;

            var observableType = Source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(observableType);
            var processMethod = conditionType.GetMethod(methodName);
            var processParameter = ExpressionHelper.MemberAccess(parameter, Selector);
            var process = BuildCall(conditionExpression, processMethod, processParameter);

            var exception = Expression.Parameter(typeof(Exception));
            var exceptionText = Expression.Property(exception, "Message");
            var runtimeException = Expression.New(runtimeExceptionConstructor, exceptionText, Expression.Constant(this), exception);
            var predicate = Expression.TryCatch(process, Expression.Catch(exception, Expression.Throw(runtimeException, process.Type)));
            return Expression.Call(whereMethod.MakeGenericMethod(observableType), Source, Expression.Lambda(predicate, parameter));
        }
    }
}
