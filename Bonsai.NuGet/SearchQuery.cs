using NuGet.Protocol.Core.Types;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    class SearchQuery : QueryContinuation<IEnumerable<IPackageSearchMetadata>>
    {
        public SearchQuery(SourceRepository repository, string searchTerm, int pageSize, bool includePrerelease, IEnumerable<string> packageTypes = default)
            : this(repository, searchTerm, 0, pageSize, includePrerelease, packageTypes)
        {
        }

        private SearchQuery(SourceRepository repository, string searchTerm, int pageIndex, int pageSize, bool includePrerelease, IEnumerable<string> packageTypes)
        {
            Repository = repository;
            SearchTerm = searchTerm;
            PageIndex = pageIndex;
            PageSize = pageSize;
            IncludePrerelease = includePrerelease;
            PackageTypes = packageTypes;
        }

        public SourceRepository Repository { get; private set; }

        public string SearchTerm { get; private set; }

        public int PageIndex { get; private set; }

        public int PageSize { get; private set; }

        public bool IncludePrerelease { get; private set; }

        public IEnumerable<string> PackageTypes { get; private set; }

        public override async Task<QueryResult<IEnumerable<IPackageSearchMetadata>>> GetResultAsync(CancellationToken token = default)
        {
            var searchFilterType = IncludePrerelease ? SearchFilterType.IsAbsoluteLatestVersion : SearchFilterType.IsLatestVersion;
            var searchFilter = new SearchFilter(IncludePrerelease, searchFilterType);
            searchFilter.PackageTypes = PackageTypes;
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
            catch (WebException e) { return QueryResult.Create(Observable.Throw<IPackageSearchMetadata>(e).ToEnumerable()); }
        }
    }
}
