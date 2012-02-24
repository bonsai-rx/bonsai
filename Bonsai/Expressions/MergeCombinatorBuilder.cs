using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    public abstract class MergeCombinatorBuilder : BinaryCombinatorExpressionBuilder
    {
        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var otherType = Other.Type.GetGenericArguments()[0];
            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", new[] { sourceType, otherType }, Source, Other);
        }

        protected abstract IObservable<Tuple<TSource, TOther>> Combine<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other);
    }

    public abstract class MergeCombinatorBuilder<TSource, TOther, TResult> : BinaryCombinatorExpressionBuilder
    {
        public override Expression Build()
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var otherType = Other.Type.GetGenericArguments()[0];
            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", null, Source, Other);
        }

        protected abstract IObservable<TResult> Combine(IObservable<TSource> source, IObservable<TOther> other);
    }
}
