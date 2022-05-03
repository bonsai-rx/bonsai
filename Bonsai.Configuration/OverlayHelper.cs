using Bonsai.NuGet;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.RuntimeModel;
using NuGet.Versioning;
using System;
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
        static readonly string[] SupportedRuntimes = new[] { "win-x86", "win-x64" };

        static RuntimeGraph GetRuntimeGraph(PackageReaderBase packageReader)
        {
            var runtimeGraphFilePath = packageReader.GetFiles().FirstOrDefault(path => string.Equals(
                Path.GetFileName(path),
                RuntimeGraph.RuntimeGraphFileName,
                StringComparison.OrdinalIgnoreCase));
            if (runtimeGraphFilePath != null)
            {
                var runtimeGraphStream = packageReader.GetStream(runtimeGraphFilePath);
                return JsonRuntimeFormat.ReadRuntimeGraph(runtimeGraphStream);
            }

            return null;
        }

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

        static NuGetVersion FindOverlayVersion(PackageReaderBase packageReader)
        {
            return (from file in packageReader.GetFiles()
                    where Path.GetFileName(file) == NuGetOverlayCommandFileName
                    from line in ReadAllLines(packageReader.GetStream(file))
                    where line.StartsWith(NuGetOverlayCommand)
                    let version = line.Split(' ')
                                      .SkipWhile(xs => xs != NuGetOverlayVersionArgument)
                                      .Skip(1)
                                      .FirstOrDefault()
                    select NuGetVersion.Parse(version)).FirstOrDefault();
        }

        public static IEnumerable<PackageIdentity> FindPivots(PackageIdentity package, PackageReaderBase packageReader)
        {
            var runtimeGraph = GetRuntimeGraph(packageReader);
            if (runtimeGraph != null)
            {
                return from runtimeName in SupportedRuntimes
                       from dependency in runtimeGraph.FindRuntimeDependencies(runtimeName, package.Id)
                       select new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion);
            }

            var overlayVersion = FindOverlayVersion(packageReader);
            if (overlayVersion != null)
            {
                return from file in packageReader.GetFiles()
                       where Path.GetFileName(file) == PivotListFileName
                       from pivot in ReadAllLines(packageReader.GetStream(file))
                       select new PackageIdentity(pivot, overlayVersion);
            }

            return Enumerable.Empty<PackageIdentity>();
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
