using NuGet.Protocol.Core.Types;

namespace Bonsai.NuGet
{
    public sealed class RequiringLicenseAcceptancePackageInfo
    {
        public RequiringLicenseAcceptancePackageInfo(IPackageSearchMetadata package, SourceRepository sourceRepository)
        {
            Package = package;
            SourceRepository = sourceRepository;
        }

        public IPackageSearchMetadata Package { get; }

        public SourceRepository SourceRepository { get; }
    }
}
