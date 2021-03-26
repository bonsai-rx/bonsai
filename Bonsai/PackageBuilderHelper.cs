using Bonsai.Configuration;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai
{
    static class PackageBuilderHelper
    {
        static string GetRelativePath(string path, string basePath)
        {
            var pathUri = new Uri(path);
            var rootUri = new Uri(basePath);
            var relativeUri = rootUri.MakeRelativeUri(pathUri);
            return relativeUri.ToString().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        static IEnumerable<ManifestFile> GetContentFiles(string basePath)
        {
            return from file in Directory.GetFiles(basePath, "*", SearchOption.AllDirectories)
                   let extension = Path.GetExtension(file)
                   where extension != NuGetConstants.ManifestExtension &&
                         extension != NuGetConstants.PackageExtension
                   select new ManifestFile
                   {
                       Source = file,
                       Target = Path.Combine("content", GetRelativePath(file, basePath))
                   };
        }

        public static Manifest CreatePackageManifest(string metadataPath)
        {
            if (File.Exists(metadataPath))
            {
                using (var stream = File.OpenRead(metadataPath))
                {
                    return Manifest.ReadFrom(stream, true);
                }
            }
            else
            {
                var metadata = new ManifestMetadata()
                {
                    Authors = new[] { Environment.UserName },
                    Version = NuGetVersion.Parse("1.0.0"),
                    Id = Path.GetFileNameWithoutExtension(metadataPath),
                    Description = "My workflow description."
                };
                return new Manifest(metadata);
            }
        }

        public static PackageBuilder CreateExecutablePackage(string path, Manifest manifest, PackageConfiguration configuration, out bool updateDependencies)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                throw new ArgumentException("Invalid workflow file path.", "path");
            }

            var packageBuilder = new PackageBuilder();
            var basePath = Path.GetDirectoryName(path) + "\\";
            packageBuilder.Populate(manifest.Metadata);
            packageBuilder.Tags.Add(NuGet.Constants.BonsaiDirectory);
            packageBuilder.Tags.Add(NuGet.Constants.GalleryDirectory);
            packageBuilder.PackageTypes = new[] { new PackageType(NuGet.Constants.GalleryPackageType, PackageType.EmptyVersion) };
            var files = manifest.Files?.Count == 0 ? GetContentFiles(basePath) : manifest.Files;
            packageBuilder.PopulateFiles(basePath, files);
            var manifestDependencies = new Dictionary<string, PackageDependency>(StringComparer.OrdinalIgnoreCase);
            foreach (var dependency in packageBuilder.DependencyGroups.Where(group => group.TargetFramework == NuGetFramework.AnyFramework)
                                                                      .SelectMany(group => group.Packages))
            {
                manifestDependencies.Add(dependency.Id, dependency);
            }

            updateDependencies = false;
            var workflowFiles = packageBuilder.Files.Select(file => file as PhysicalPackageFile)
                                                    .Where(file => file != null && Path.GetExtension(file.SourcePath) == NuGet.Constants.BonsaiExtension)
                                                    .Select(file => file.SourcePath)
                                                    .ToArray();
            var workflowDependencies = DependencyInspector.GetWorkflowPackageDependencies(workflowFiles, configuration).ToArray().Wait();
            foreach (var dependency in workflowDependencies)
            {
                if (!manifestDependencies.TryGetValue(dependency.Id, out PackageDependency manifestDependency) ||
                    !DependencyEqualityComparer.Default.Equals(dependency, manifestDependency))
                {
                    updateDependencies = true;
                    manifestDependencies[dependency.Id] = dependency;
                }
            }

            var dependencyGroup = new PackageDependencyGroup(NuGetFramework.AnyFramework, manifestDependencies.Values);
            packageBuilder.DependencyGroups.Clear();
            packageBuilder.DependencyGroups.Add(dependencyGroup);
            return packageBuilder;
        }

        class DependencyEqualityComparer : IEqualityComparer<PackageDependency>
        {
            public static readonly DependencyEqualityComparer Default = new DependencyEqualityComparer();

            public bool Equals(PackageDependency x, PackageDependency y)
            {
                return x.Id.Equals(y.Id, StringComparison.OrdinalIgnoreCase) &&
                       x.VersionRange.IsMaxInclusive == y.VersionRange.IsMaxInclusive &&
                       x.VersionRange.IsMinInclusive == y.VersionRange.IsMinInclusive &&
                       x.VersionRange.MaxVersion == y.VersionRange.MaxVersion &&
                       x.VersionRange.MinVersion == y.VersionRange.MinVersion;
            }

            public int GetHashCode(PackageDependency obj)
            {
                return obj.Id.GetHashCode() ^
                       obj.VersionRange.IsMaxInclusive.GetHashCode() ^
                       obj.VersionRange.IsMinInclusive.GetHashCode() ^
                       EqualityComparer<SemanticVersion>.Default.GetHashCode(obj.VersionRange.MaxVersion) ^
                       EqualityComparer<SemanticVersion>.Default.GetHashCode(obj.VersionRange.MinVersion);
            }
        }
    }
}
