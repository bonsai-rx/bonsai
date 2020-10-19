using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    static class AggregateQuery
    {
        public static AggregateQuery<TResult> Create<TResult>(IEnumerable<QueryContinuation<TResult>> queries, Func<IEnumerable<TResult>, TResult> aggregator)
        {
            return new AggregateQuery<TResult>(queries, aggregator);
        }
    }

    class AggregateQuery<TResult> : QueryContinuation<TResult>
    {
        public AggregateQuery(IEnumerable<QueryContinuation<TResult>> queries, Func<IEnumerable<TResult>, TResult> aggregator)
        {
            Queries = queries;
            Aggregator = aggregator;
        }

        private IEnumerable<QueryContinuation<TResult>> Queries { get; set; }

        private Func<IEnumerable<TResult>, TResult> Aggregator { get; set; }

        public override async Task<QueryResult<TResult>> GetResultAsync(CancellationToken token = default)
        {
            var taskResults = await Task.WhenAll(Queries.Select(q => q.GetResultAsync(token)));
            var continuations = default(List<QueryContinuation<TResult>>);
            var results = new List<TResult>();
            foreach (var queryResult in taskResults)
            {
                results.Add(queryResult.Result);
                if (queryResult.Continuation != null)
                {
                    if (continuations == null) continuations = new List<QueryContinuation<TResult>>();
                    continuations.Add(queryResult.Continuation);
                }
            }

            var aggregateResult = Aggregator(results);
            var continuation = continuations != null ? new AggregateQuery<TResult>(continuations, Aggregator) : null;
            return QueryResult.Create(aggregateResult, continuation);
        }
    }
}
