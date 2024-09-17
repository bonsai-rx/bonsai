using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet.Search
{
    internal class PackageTypeSearchResourceV3 : PackageSearchResource
    {
        readonly PackageSearchResourceV3 packageSearchResource;

        public PackageTypeSearchResourceV3(PackageSearchResourceV3 searchResource)
        {
            packageSearchResource = searchResource ?? throw new ArgumentNullException(nameof(searchResource));
        }

        public override Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(string searchTerm, SearchFilter filters, int skip, int take, ILogger log, CancellationToken cancellationToken)
        {
            if (filters?.PackageTypes != null
                && filters.PackageTypes.SingleOrDefault() is string packageType)
            {
                searchTerm += "&packageType=" + packageType;
            }

            return packageSearchResource.SearchAsync(searchTerm, filters, skip, take, log, cancellationToken);
        }
    }
}
