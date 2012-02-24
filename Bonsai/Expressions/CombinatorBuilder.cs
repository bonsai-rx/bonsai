using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    public abstract class CombinatorBuilder : CombinatorExpressionBuilder
    {
        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments();
            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", observableType, Source);
        }

        protected abstract IObservable<TSource> Combine<TSource>(IObservable<TSource> source);
    }

    public abstract class CombinatorBuilder<TResult> : CombinatorExpressionBuilder
    {
        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments();
            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", observableType, Source);
        }

        protected abstract IObservable<TResult> Combine<TSource>(IObservable<TSource> source);
    }

    public abstract class CombinatorBuilder<TSource, TResult> : CombinatorExpressionBuilder
    {
        public override Expression Build()
        {
            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", null, Source);
        }

        protected abstract IObservable<TResult> Combine(IObservable<TSource> source);
    }
}
