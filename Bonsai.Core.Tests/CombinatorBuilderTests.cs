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

        IObservable<TResult> TestCombinatorBuilder<TResult, TCombinator>(params Expression[] arguments)
            where TCombinator : new()
        {
            var combinator = new TCombinator();
            return TestCombinatorBuilder<TResult>(combinator, arguments);
        }

        IObservable<TResult> TestCombinatorBuilder<TResult>(object combinator, params Expression[] arguments)
        {
            Expression buildResult;
            var builder = new CombinatorBuilder { Combinator = combinator };
            try { buildResult = builder.Build(arguments); }
            catch (Exception ex) { throw new WorkflowBuildException(ex.Message, builder, ex); }
            if (buildResult.Type != typeof(IObservable<TResult>))
            {
                var actualType = buildResult.Type.GetGenericArguments()[0];
                throw new WorkflowBuildException($"" +
                    $"Output signature does not match. Expected: {typeof(TResult)}. Actual: {actualType}.");
            }

            var lambda = Expression.Lambda<Func<IObservable<TResult>>>(buildResult);
            var resultFactory = lambda.Compile();
            var result = resultFactory();
            return result;
        }
    }
}
