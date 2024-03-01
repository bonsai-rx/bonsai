using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    public class SearchQuery : QueryContinuation<IEnumerable<IPackageSearchMetadata>>
    {
        public SearchQuery(
            SourceRepository repository,
            string searchTerm,
            int pageSize,
            bool includePrerelease,
            IEnumerable<string> packageTypes = default)
            : this(repository, searchTerm, 0, pageSize, includePrerelease, packageTypes)
        {
        }

        private SearchQuery(
            SourceRepository repository,
            string searchTerm,
            int pageIndex,
            int pageSize,
            bool includePrerelease,
            IEnumerable<string> packageTypes)
        {
            Repository = repository;
            SearchTerm = searchTerm;
            PageIndex = pageIndex;
            PageSize = pageSize;
            IncludePrerelease = includePrerelease;
            PackageTypes = packageTypes;
        }

        public SourceRepository Repository { get; }

        public string SearchTerm { get; }

        public int PageIndex { get; }

        public int PageSize { get; }

        public bool IncludePrerelease { get; }

        public IEnumerable<string> PackageTypes { get; }

        public override async Task<QueryResult<IEnumerable<IPackageSearchMetadata>>> GetResultAsync(CancellationToken token = default)
        {
            var searchFilter = QueryHelper.CreateSearchFilter(IncludePrerelease, PackageTypes);
            try
            {
                var result = (await Repository.SearchAsync(SearchTerm, searchFilter, PageIndex * PageSize, PageSize + 1, token)).ToList();
                var continuation = result.Count > PageSize ? new SearchQuery(
                    repository: Repository,
                    searchTerm: SearchTerm,
                    pageIndex: PageIndex + 1,
                    pageSize: PageSize,
                    includePrerelease: IncludePrerelease,
                    packageTypes: PackageTypes) : null;
                return QueryResult.Create(result.Take(PageSize), continuation);
            }
            catch (NuGetProtocolException ex)
            {
                var exception = new InvalidOperationException($"There was an error searching the repository '{Repository}'.", ex);
                return QueryResult.Create(Observable.Throw<IPackageSearchMetadata>(exception).ToEnumerable());
            }
        }
    }
}
