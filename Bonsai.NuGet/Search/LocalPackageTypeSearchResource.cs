using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet.Search
{
    internal class LocalPackageTypeSearchResource : PackageSearchResource
    {
        readonly FindLocalPackagesResource findLocalPackagesResource;
        readonly Lazy<LocalPackageSearchResource> defaultSearchResource;

        public LocalPackageTypeSearchResource(FindLocalPackagesResource localResource)
        {
            findLocalPackagesResource = localResource ?? throw new ArgumentNullException(nameof(localResource));
            defaultSearchResource = new Lazy<LocalPackageSearchResource>(
                () => new LocalPackageSearchResource(findLocalPackagesResource));
        }

        public override async Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(
            string searchTerm,
            SearchFilter filters,
            int skip,
            int take,
            ILogger log,
            CancellationToken cancellationToken)
        {
            LocalPackageSearchResource searchResource;
            if (filters?.PackageTypes != null
                && filters.PackageTypes.SingleOrDefault() is string packageType)
            {
                var wrapperResource = new FindLocalPackageWrapperResource(findLocalPackagesResource, packageType);
                searchResource = new LocalPackageSearchResource(wrapperResource);
            }
            else searchResource = defaultSearchResource.Value;

            return await searchResource.SearchAsync(searchTerm, filters, skip, take, log, cancellationToken);
        }

        class FindLocalPackageWrapperResource : FindLocalPackagesResource
        {
            readonly FindLocalPackagesResource baseLocalResource;

            public FindLocalPackageWrapperResource(FindLocalPackagesResource localResource, string packageType)
            {
                baseLocalResource = localResource ?? throw new ArgumentNullException(nameof(localResource));
                Root = baseLocalResource.Root;
                PackageTypeFilter = packageType;
            }

            public string PackageTypeFilter { get; }

            public override IEnumerable<LocalPackageInfo> FindPackagesById(string id, ILogger logger, CancellationToken token)
            {
                return baseLocalResource.FindPackagesById(id, logger, token);
            }

            public override LocalPackageInfo GetPackage(Uri path, ILogger logger, CancellationToken token)
            {
                throw new NotSupportedException();
            }

            public override LocalPackageInfo GetPackage(PackageIdentity identity, ILogger logger, CancellationToken token)
            {
                throw new NotSupportedException();
            }

            public override IEnumerable<LocalPackageInfo> GetPackages(ILogger logger, CancellationToken token)
            {
                var result = baseLocalResource.GetPackages(logger, token);
                if (!string.IsNullOrEmpty(PackageTypeFilter))
                {
                    result = result.Where(package => package.IsPackageType(PackageTypeFilter));
                }
                return result;
            }
        }
    }
}
