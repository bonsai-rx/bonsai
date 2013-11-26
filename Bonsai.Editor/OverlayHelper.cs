using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    static class OverlayHelper
    {
        const string PivotListFileName = "pivot-list.txt";

        static IEnumerable<string> ReadAllLines(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    yield return reader.ReadLine();
                }
            }
        }

        public static IEnumerable<string> FindPivots(IPackage package, string installPath)
        {
            return from file in package.GetFiles()
                   where Path.GetFileName(file.Path) == PivotListFileName
                   from pivot in ReadAllLines(file.GetStream())
                   select pivot;
        }

        public static PackageManager CreateOverlayManager(IPackageRepository sourceRepository, string installPath)
        {
            var fileSystem = new PhysicalFileSystem(installPath);
            var overlayPathResolver = new OverlayPackagePathResolver(installPath);
            return new PackageManager(sourceRepository, overlayPathResolver, fileSystem);
        }
    }
}
