using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    public abstract class WindowBuilder : CombinatorExpressionBuilder
    {
        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments();
            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", observableType, Source);
        }

        protected abstract IObservable<IObservable<TSource>> Combine<TSource>(IObservable<TSource> source);
    }
}
