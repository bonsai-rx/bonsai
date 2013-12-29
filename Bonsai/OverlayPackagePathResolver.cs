using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    class OverlayPackagePathResolver : DefaultPackagePathResolver
    {
        public string OverlayDirectory { get; set; }

        public OverlayPackagePathResolver(string path)
            : base(path)
        {
            OverlayDirectory = string.Empty;
        }

        public override string GetPackageDirectory(IPackage package)
        {
            return GetPackageDirectory(package.Id, package.Version);
        }

        public override string GetPackageDirectory(string packageId, SemanticVersion version)
        {
            return OverlayDirectory;
        }
    }
}
