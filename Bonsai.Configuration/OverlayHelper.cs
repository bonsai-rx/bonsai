using Bonsai.NuGet;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Versioning;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bonsai.Configuration
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

        public static NuGetVersion FindOverlayVersion(PackageReaderBase package)
        {
            return (from file in package.GetFiles()
                    where Path.GetFileName(file) == NuGetOverlayCommandFileName
                    from line in ReadAllLines(package.GetStream(file))
                    where line.StartsWith(NuGetOverlayCommand)
                    let version = line.Split(' ')
                                      .SkipWhile(xs => xs != NuGetOverlayVersionArgument)
                                      .Skip(1)
                                      .FirstOrDefault()
                    select NuGetVersion.Parse(version)).FirstOrDefault();
        }

        public static IEnumerable<string> FindPivots(PackageReaderBase package, string installPath)
        {
            return from file in package.GetFiles()
                   where Path.GetFileName(file) == PivotListFileName
                   from pivot in ReadAllLines(package.GetStream(file))
                   select pivot;
        }

        public static PackageManager CreateOverlayManager(IPackageManager packageManager, string installPath)
        {
            var overlayPackageSource = new PackageSource(installPath);
            var overlayPathResolver = new OverlayPackagePathResolver(installPath);
            var overlayManager = new PackageManager(
                packageManager.Settings,
                packageManager.SourceRepositoryProvider.PackageSourceProvider,
                overlayPackageSource,
                overlayPathResolver);
            overlayManager.PackageSaveMode = PackageSaveMode.Defaultv2;
            return overlayManager;
        }
    }
}
