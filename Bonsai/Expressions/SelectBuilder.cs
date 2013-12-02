using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    [SourceMapping]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class SelectBuilder : CombinatorExpressionBuilder
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        [Browsable(false)]
        public string MemberSelector { get; set; }

        protected abstract Expression BuildSelector(Expression expression);

        public override Expression Build()
        {
            var source = Arguments.Values.Single();
            var parameterType = source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(parameterType);
            var memberExpression = ExpressionHelper.MemberAccess(parameter, MemberSelector);
            var selectorBody = BuildSelector(memberExpression);
            var selector = Expression.Lambda(selectorBody, parameter);
            return Expression.Call(selectMethod.MakeGenericMethod(parameterType, selectorBody.Type), source, selector);
        }
    }
}
