using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Core.Tests
{
    [TestClass]
    public partial class CombinatorBuilderTests
    {
        async Task<TResult> Last<TResult>(IObservable<TResult> source)
        {
            return await source.LastAsync();
        }

        Expression CreateObservableExpression<TSource>(IObservable<TSource> source)
        {
            return Expression.Constant(source, typeof(IObservable<TSource>));
        }

        IObservable<TSource> TestCombinatorBuilder<TSource>(object combinator, params Expression[] arguments)
        {
            var builder = new CombinatorBuilder { Combinator = combinator };
            var buildResult = builder.Build(arguments);
            var lambda = Expression.Lambda<Func<IObservable<TSource>>>(buildResult);
            var resultFactory = lambda.Compile();
            var result = resultFactory();
            return result;
        }
    }
}
