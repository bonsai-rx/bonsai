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
    [WorkflowElementCategory(ElementCategory.Transform)]
    [XmlType("Select", Namespace = Constants.XmlNamespace)]
    public class SelectBuilder : CombinatorExpressionBuilder
    {
        static readonly ConstructorInfo runtimeExceptionConstructor = typeof(WorkflowRuntimeException).GetConstructor(new[] { typeof(string), typeof(ExpressionBuilder), typeof(Exception) });
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public object Selector { get; set; }

        public override Expression Build()
        {
            var selectorType = Selector.GetType();
            var selectorExpression = Expression.Constant(Selector);
            var selectorAttributes = selectorType.GetCustomAttributes(typeof(SelectorAttribute), true);
            var methodName = ((SelectorAttribute)selectorAttributes.Single()).MethodName;
            var processMethods = selectorType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                              .Where(m => m.Name == methodName && m.GetParameters().Length == 1);
            var parameter = Expression.Parameter(Source.Type.GetGenericArguments()[0]);
            var process = BuildCall(selectorExpression, processMethods, parameter);
            return Expression.Call(selectMethod.MakeGenericMethod(parameter.Type, process.Type), Source, Expression.Lambda(process, parameter));
        }
    }
}
