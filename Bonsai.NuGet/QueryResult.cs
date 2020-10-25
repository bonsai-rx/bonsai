namespace Bonsai.NuGet
{
    public static class QueryResult
    {
        public static QueryResult<TResult> Create<TResult>(TResult result, QueryContinuation<TResult> continuation = null)
        {
            return new QueryResult<TResult>(result, continuation);
        }
    }

    public class QueryResult<TResult>
    {
        public QueryResult(TResult result, QueryContinuation<TResult> continuation)
        {
            Result = result;
            Continuation = continuation;
        }

        public TResult Result { get; private set; }

        public QueryContinuation<TResult> Continuation { get; private set; }
    }
}
