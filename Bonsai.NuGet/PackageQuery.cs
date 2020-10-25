using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    public class PackageQuery
    {
        readonly PackageQueryIndex queryIndex;
        QueryContinuation<IEnumerable<IPackageSearchMetadata>> continuation;
        int continuationCounter;

        public PackageQuery(string searchTerm, int pageSize, QueryContinuation<IEnumerable<IPackageSearchMetadata>> query)
        {
            var comparer = new RelevanceSearchMetadataComparer(searchTerm);
            queryIndex = new PackageQueryIndex(comparer) { PageSize = pageSize };
            continuation = query;
        }

        public bool HasQueryPage(int pageIndex)
        {
            return continuationCounter > pageIndex;
        }

        public bool IsCompleted
        {
            get { return continuation == null; }
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> GetPackageFeed(int pageIndex, CancellationToken token = default)
        {
            if (continuation != null && pageIndex >= continuationCounter)
            {
                var queryResult = await continuation.GetResultAsync(token);
                queryIndex.AddRange(queryResult.Result);
                continuation = queryResult.Continuation;
                continuationCounter++;
            }
            return queryIndex.GetPage(pageIndex);
        }
    }
}
