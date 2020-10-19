using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;

namespace Bonsai.NuGet
{
    class RelevanceSearchMetadataComparer : IComparer<IPackageSearchMetadata>
    {
        public static readonly RelevanceSearchMetadataComparer Default = new RelevanceSearchMetadataComparer();

        public RelevanceSearchMetadataComparer()
        {
        }

        public RelevanceSearchMetadataComparer(string searchTerm)
        {
            SearchTerm = searchTerm;
        }

        public string SearchTerm { get; private set; }

        public int Compare(IPackageSearchMetadata x, IPackageSearchMetadata y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                //TODO: search term relevance ranking
            }

            var downloads = -Comparer<long?>.Default.Compare(x.DownloadCount, y.DownloadCount);
            if (downloads != 0) return downloads;

            return PackageIdentityComparer.Default.Compare(x.Identity, y.Identity);
        }
    }
}
