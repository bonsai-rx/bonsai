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
            var memberAccess = FindMemberAccess(Selector);
            var memberSelector = ExpressionHelper.MemberAccess(parameter, memberAccess.Item2);

            var conditionParameter = Expression.Parameter(typeof(IObservable<>).MakeGenericType(memberSelector.Type));
            var processMethods = conditionType.GetMethods().Where(m => m.Name == methodName);
            var processCall = BuildCall(conditionExpression, processMethods, conditionParameter);
            return (Expression)Expression.Call(
                typeof(ConditionBuilder),
                "Process",
                new[] { observableType, memberSelector.Type },
                sourceSelect,
                Expression.Lambda(memberSelector, parameter),
                Expression.Lambda(processCall, conditionParameter));
        }

        static IObservable<TSource> Process<TSource, TMember>(IObservable<TSource> source, Func<TSource, TMember> selector, Func<IObservable<TMember>, IObservable<bool>> condition)
        {
            return Observable.Defer(() =>
            {
                var filter = false;
                TSource value = default(TSource);
                return condition(source.Select(xs => { value = xs; return selector(xs); }))
                    .Select(bs => { filter = bs; return value; })
                    .Where(xs => filter);
            });
        }
    }
}
