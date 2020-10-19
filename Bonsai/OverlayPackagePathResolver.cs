using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace Bonsai
{
    class OverlayPackagePathResolver : PackagePathResolver
    {
        public OverlayPackagePathResolver(string path)
            : base(path)
        {
            OverlayDirectory = string.Empty;
        }

        public string OverlayDirectory { get; set; }

        public override string GetPackageDirectoryName(PackageIdentity package)
        {
            return OverlayDirectory;
        }
    }
}
