using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet.Search
{
    internal class PackageTypeSearchResourceV3Provider : ResourceProvider
    {
        readonly PackageSearchResourceV3Provider searchResourceV3Provider = new();

        public PackageTypeSearchResourceV3Provider()
            : base(typeof(PackageSearchResource), nameof(PackageTypeSearchResourceV3Provider), nameof(PackageSearchResourceV3Provider))
        {
        }

        public override async Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token)
        {
            INuGetResource resource = null;
            var result = await searchResourceV3Provider.TryCreate(source, token);
            if (result.Item1)
            {
                var searchResource = (PackageSearchResourceV3)result.Item2;
                resource = new PackageTypeSearchResourceV3(searchResource);
            }

            return new Tuple<bool, INuGetResource>(resource != null, resource);
        }
    }
}
