using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
