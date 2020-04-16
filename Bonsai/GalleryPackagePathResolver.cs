using NuGet;

namespace Bonsai
{
    class GalleryPackagePathResolver : DefaultPackagePathResolver
    {
        public GalleryPackagePathResolver(string path)
            : base(path)
        {
        }

        public override string GetPackageDirectory(string packageId, SemanticVersion version)
        {
            return string.Empty;
        }
    }
}
