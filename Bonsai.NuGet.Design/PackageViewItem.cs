using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet.Design
{
    internal class PackageViewItem
    {
        public PackageViewItem(
            IPackageSearchMetadata selectedPackage,
            IEnumerable<VersionInfo> packageVersions,
            PackageDeprecationMetadata deprecationMetadata,
            SourceRepository sourceRepository,
            LocalPackageInfo localPackage,
            ImageList imageList,
            int imageIndex)
        {
            SelectedPackage = selectedPackage;
            PackageVersions = packageVersions;
            DeprecationMetadata = deprecationMetadata;
            SourceRepository = sourceRepository;
            LocalPackage = localPackage;
            ImageList = imageList;
            ImageIndex = imageIndex;
        }

        public IPackageSearchMetadata SelectedPackage { get; }

        public IEnumerable<VersionInfo> PackageVersions { get; }

        public PackageDeprecationMetadata DeprecationMetadata { get; }

        public SourceRepository SourceRepository { get; }

        public LocalPackageInfo LocalPackage { get; }

        public ImageList ImageList { get; }

        public int ImageIndex { get; }

        public async Task<IPackageSearchMetadata> GetVersionMetadataAsync(VersionInfo versionInfo, CancellationToken cancellationToken = default)
        {
            using var cacheContext = new SourceCacheContext();
            var identity = new PackageIdentity(SelectedPackage.Identity.Id, versionInfo.Version);
            return await SourceRepository.GetMetadataAsync(identity, cacheContext, cancellationToken);
        }
    }
}
