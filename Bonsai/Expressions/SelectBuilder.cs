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
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class SelectBuilder : SingleArgumentExpressionBuilder
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        protected abstract Expression BuildSelector(Expression expression);

        public override Expression Build()
        {
            var source = Arguments.Single();
            var parameterType = source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(parameterType);
            var selectorBody = BuildSelector(parameter);
            var selector = Expression.Lambda(selectorBody, parameter);
            return Expression.Call(selectMethod.MakeGenericMethod(parameterType, selectorBody.Type), source, selector);
        }
    }
}
