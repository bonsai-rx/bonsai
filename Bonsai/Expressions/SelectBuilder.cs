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
    [SourceMapping]
    [PropertyMapping]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [XmlType("Select", Namespace = Constants.XmlNamespace)]
    public class SelectBuilder : CombinatorExpressionBuilder
    {
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public object Selector { get; set; }

        public string MemberSelector { get; set; }

        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        public override Expression Build()
        {
            var selectorType = Selector.GetType();
            var selectorExpression = Expression.Constant(Selector);
            var selectorAttributes = selectorType.GetCustomAttributes(typeof(SelectorAttribute), true);
            var methodName = ((SelectorAttribute)selectorAttributes.Single()).MethodName;
            var processMethods = selectorType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                              .Where(m => m.Name == methodName && m.GetParameters().Length == 1);
            return BuildCallRemapping(
                selectorExpression,
                (selector, sourceSelect) =>
                {
                    var parameter = Expression.Parameter(sourceSelect.Type.GetGenericArguments()[0]);
                    var process = BuildCall(selector, processMethods, parameter);
                    return Expression.Call(selectMethod.MakeGenericMethod(parameter.Type, process.Type), sourceSelect, Expression.Lambda(process, parameter));
                },
                Arguments.Values.Single(),
                MemberSelector,
                propertyMappings);
        }
    }
}
