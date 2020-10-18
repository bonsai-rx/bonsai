using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;

namespace Bonsai.NuGet
{
    class PackageQueryIndex
    {
        readonly SortedList<IPackageSearchMetadata, IPackageSearchMetadata> metadata;
        readonly Dictionary<string, IPackageSearchMetadata> identities;

        public PackageQueryIndex(IComparer<IPackageSearchMetadata> comparer)
        {
            metadata = new SortedList<IPackageSearchMetadata, IPackageSearchMetadata>(comparer);
            identities = new Dictionary<string, IPackageSearchMetadata>();
        }

        public int PageSize { get; set; }

        public void AddRange(IEnumerable<IPackageSearchMetadata> collection)
        {
            foreach (var result in collection)
            {
                var packageId = result.Identity.Id;
                if (identities.TryGetValue(packageId, out IPackageSearchMetadata package))
                {
                    if (package.Identity.Version >= result.Identity.Version) continue;
                    metadata.Remove(package);
                    identities[packageId] = result;
                }
                else identities.Add(packageId, result);
                metadata[result] = result;
            }
        }

        public IEnumerable<IPackageSearchMetadata> GetPage(int pageIndex)
        {
            var offset = pageIndex * PageSize;
            for (int i = offset; i < Math.Min(metadata.Count, offset + PageSize); i++)
            {
                yield return metadata.Values[i];
            }
        }
    }
}
