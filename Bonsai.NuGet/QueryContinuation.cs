using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    public abstract class QueryContinuation<TResult>
    {
        public abstract Task<QueryResult<TResult>> GetResultAsync(CancellationToken token = default);
    }

    public static class QueryContinuation
    {
        public static QueryContinuation<TResult> FromResult<TResult>(TResult result)
        {
            return new ResultQueryContinuation<TResult>(result);
        }

        class ResultQueryContinuation<TResult> : QueryContinuation<TResult>
        {
            public ResultQueryContinuation(TResult result)
            {
                Result = result;
            }

            public TResult Result { get; private set; }

            public override Task<QueryResult<TResult>> GetResultAsync(CancellationToken token = default)
            {
                return Task.FromResult(QueryResult.Create(Result));
            }
        }
    }
}
