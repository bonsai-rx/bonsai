using NuGet.Protocol;
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
        public UpdateQuery(SourceRepository repository, IEnumerable<LocalPackageInfo> localPackages, bool includePrerelease)
        {
            Repository = repository;
            LocalPackages = localPackages;
            IncludePrerelease = includePrerelease;
        }

        public SourceRepository Repository { get; private set; }

        public IEnumerable<LocalPackageInfo> LocalPackages { get; private set; }

        public bool IncludePrerelease { get; private set; }

        public override async Task<QueryResult<IEnumerable<IPackageSearchMetadata>>> GetResultAsync(CancellationToken token = default)
        {
            try { return QueryResult.Create(await Repository.GetUpdatesAsync(LocalPackages, IncludePrerelease, token)); }
            catch (NuGetProtocolException ex)
            {
                var exception = new InvalidOperationException($"There was an error accessing the repository '{Repository}'.", ex);
                return QueryResult.Create(Observable.Throw<IPackageSearchMetadata>(exception).ToEnumerable());
            }
        }
    }
}
