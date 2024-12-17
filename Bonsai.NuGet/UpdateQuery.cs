using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
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
            string packageType = default)
        {
            RemoteRepository = remoteRepository;
            LocalRepository = localRepository;
            SearchTerm = searchTerm;
            IncludePrerelease = includePrerelease;
            PackageType = packageType;
        }

        public SourceRepository RemoteRepository { get; }

        public SourceRepository LocalRepository { get; }

        public string SearchTerm { get; }

        public bool IncludePrerelease { get; }

        public string PackageType { get; }

        public override async Task<QueryResult<IEnumerable<IPackageSearchMetadata>>> GetResultAsync(CancellationToken token = default)
        {
            try
            {
                var localSearchFilter = QueryHelper.CreateSearchFilter(includePrerelease: true, PackageType);
                var localPackages = await LocalRepository.SearchAsync(SearchTerm, localSearchFilter, token: token);
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
