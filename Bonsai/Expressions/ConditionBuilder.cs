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
    public class ConditionBuilder : CombinatorExpressionBuilder, INamedElement
    {
        static readonly MethodInfo whereMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Where" &&
                                                                           m.GetParameters().Length == 2 &&
                                                                           m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public ConditionBuilder()
            : base(minArguments: 1, maxArguments: 1)
        {
        }

        public string Name
        {
            get { return GetElementDisplayName(Condition); }
        }
        
        public object Condition { get; set; }

        [Description("The inner property on which to apply the condition.")]
        [Editor("Bonsai.Design.MemberSelectorEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Selector { get; set; }

        public override Expression Build()
        {
            var output = BuildCombinator();
            var conditionExpression = Expression.Constant(Condition);
            return BuildMappingOutput(conditionExpression, output, PropertyMappings);
        }

        protected override Expression BuildCombinator()
        {
            var conditionType = Condition.GetType();
            var conditionExpression = Expression.Constant(Condition);
            var conditionAttributes = conditionType.GetCustomAttributes(typeof(CombinatorAttribute), true);
            var methodName = ((CombinatorAttribute)conditionAttributes.Single()).MethodName;

            var sourceSelect = Arguments.Values.First();
            var observableType = sourceSelect.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(observableType);
            var processMethods = conditionType.GetMethods().Where(m => m.Name == methodName);
            var memberAccess = FindMemberAccess(Selector);
            var processParameter = ExpressionHelper.MemberAccess(parameter, memberAccess.Item2);
            return (Expression)Expression.Call(
                typeof(ConditionBuilder),
                "Process",
                new[] { observableType, processParameter.Type },
                conditionExpression,
                sourceSelect,
                Expression.Lambda(processParameter, parameter));
        }

        static IObservable<TSource> Process<TSource, TMember>(Condition<TMember> condition, IObservable<TSource> source, Func<TSource, TMember> selector)
        {
            return Observable.Defer(() =>
            {
                var filter = false;
                TSource value = default(TSource);
                return condition
                    .Process(source.Select(xs => { value = xs; return selector(xs); }))
                    .Select(bs => { filter = bs; return value; })
                    .Where(xs => filter);
            });
        }
    }
}
