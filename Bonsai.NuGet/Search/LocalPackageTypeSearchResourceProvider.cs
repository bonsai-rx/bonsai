using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet.Search
{
    internal class LocalPackageTypeSearchResourceProvider : ResourceProvider
    {
        public LocalPackageTypeSearchResourceProvider()
            : base(typeof(PackageSearchResource), nameof(LocalPackageTypeSearchResourceProvider), nameof(LocalPackageSearchResourceProvider))
        {
        }

        public override async Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
        {
            INuGetResource resource = null;
            var localResource = await source.GetResourceAsync<FindLocalPackagesResource>(token);
            if (localResource != null)
            {
                resource = new LocalPackageTypeSearchResource(localResource);
            }

            return new Tuple<bool, INuGetResource>(resource != null, resource);
        }
    }
}
