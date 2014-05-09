using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    static class OverlayHelper
    {
        const string PivotListFileName = "pivot-list.txt";
        const string NuGetOverlayCommandFileName = "NuGet-Overlay.cmd";
        const string NuGetOverlayCommand = "nuget overlay";
        const string NuGetOverlayVersionArgument = "-Version";

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

        static SemanticVersion TryParseVersion(string version)
        {
            SemanticVersion value;
            SemanticVersion.TryParse(version, out value);
            return value;
        }

        public static SemanticVersion FindOverlayVersion(IPackage package)
        {
            return (from file in package.GetFiles()
                    where Path.GetFileName(file.Path) == NuGetOverlayCommandFileName
                    from line in ReadAllLines(file.GetStream())
                    where line.StartsWith(NuGetOverlayCommand)
                    let version = line.Split(' ')
                                      .SkipWhile(xs => xs != NuGetOverlayVersionArgument)
                                      .Skip(1)
                                      .FirstOrDefault()
                    select TryParseVersion(version)).FirstOrDefault();
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
