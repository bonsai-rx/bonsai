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
    [WorkflowElementCategory(ElementCategory.Condition)]
    [XmlType("Where", Namespace = Constants.XmlNamespace)]
    public class WhereBuilder : CombinatorExpressionBuilder
    {
        readonly PropertyMappingCollection propertyMappings = new PropertyMappingCollection();
        static readonly MethodInfo whereMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Where" &&
                                                                           m.GetParameters().Length == 2 &&
                                                                           m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public object Predicate { get; set; }

        [Description("The inner property on which to apply the condition.")]
        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Selector { get; set; }

        public string MemberSelector { get; set; }

        public PropertyMappingCollection PropertyMappings
        {
            get { return propertyMappings; }
        }

        public override Expression Build()
        {
            var predicateType = Predicate.GetType();
            var predicateExpression = Expression.Constant(Predicate);
            var predicateAttributes = predicateType.GetCustomAttributes(typeof(PredicateAttribute), true);
            var methodName = ((PredicateAttribute)predicateAttributes.Single()).MethodName;
            return BuildCallRemapping(
                predicateExpression,
                (predicate, sourceSelect) =>
                {
                    var observableType = sourceSelect.Type.GetGenericArguments()[0];
                    var parameter = Expression.Parameter(observableType);
                    var processMethods = predicateType.GetMethods().Where(m => m.Name == methodName);
                    var processParameter = ExpressionHelper.MemberAccess(parameter, Selector);
                    var process = BuildCall(predicate, processMethods, processParameter);
                    return Expression.Call(whereMethod.MakeGenericMethod(observableType), sourceSelect, Expression.Lambda(process, parameter));
                },
                Source,
                MemberSelector,
                propertyMappings);
        }
    }
}
