using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    public class UpdateQuery : QueryContinuation<IEnumerable<IPackageSearchMetadata>>
    {
        public UpdateQuery(
            SourceRepository remoteRepository,
            SourceRepository localRepository,
            string searchTerm,
            bool includePrerelease,
            IEnumerable<string> packageTypes = default)
        {
            RemoteRepository = remoteRepository;
            LocalRepository = localRepository;
            SearchTerm = searchTerm;
            IncludePrerelease = includePrerelease;
            PackageTypes = packageTypes;
        }

        public SourceRepository RemoteRepository { get; }

        public SourceRepository LocalRepository { get; }

        public string SearchTerm { get; }

        public bool IncludePrerelease { get; }

        public IEnumerable<string> PackageTypes { get; }

        public override async Task<QueryResult<IEnumerable<IPackageSearchMetadata>>> GetResultAsync(CancellationToken token = default)
        {
            try
            {
                var searchFilter = QueryHelper.CreateSearchFilter(IncludePrerelease, PackageTypes);
                var localPackages = await LocalRepository.SearchAsync(SearchTerm, searchFilter, 0, int.MaxValue, token);
                return QueryResult.Create(await RemoteRepository.GetUpdatesAsync(localPackages, IncludePrerelease, token));
            }
            catch (NuGetProtocolException ex)
            {
                var exception = new InvalidOperationException($"There was an error accessing the repository '{RemoteRepository}'.", ex);
                return QueryResult.Create(Observable.Throw<IPackageSearchMetadata>(exception).ToEnumerable());
            }
        }
    }
}
