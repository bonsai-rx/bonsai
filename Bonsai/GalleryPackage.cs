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
    static class GalleryPackage
    {
        static readonly string ExcludeFiles =
            $@"**\*{NuGetConstants.ManifestExtension};" +
            $@"**\*{NuGetConstants.PackageExtension};" +
            $@"**\{NuGet.Constants.BonsaiExtension}\**";

        public static Manifest CreateManifest(string metadataPath)
        {
            if (File.Exists(metadataPath))
            {
                using var stream = File.OpenRead(metadataPath);
                return Manifest.ReadFrom(stream, true);
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

        public static PackageBuilder CreatePackageBuilder(string path, Manifest manifest, PackageConfiguration configuration, out bool updateDependencies)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                throw new ArgumentException("Invalid workflow file path.", nameof(path));
            }

            var packageBuilder = new PackageBuilder();
            var basePath = Path.GetDirectoryName(path) + "\\";
            packageBuilder.Populate(manifest.Metadata);
            packageBuilder.Tags.Add(NuGet.Constants.BonsaiTag);
            packageBuilder.Tags.Add(NuGet.Constants.GalleryTag);
            packageBuilder.PackageTypes = new[] { new PackageType(NuGet.Constants.GalleryPackageType, PackageType.EmptyVersion) };
            if (packageBuilder.LicenseMetadata is not null)
                packageBuilder.LicenseUrl = null;

            if (manifest.HasFilesNode)
                packageBuilder.PopulateFiles(basePath, manifest.Files);
            else
                packageBuilder.AddFiles(basePath, "**", PackagingConstants.Folders.Content, ExcludeFiles);

            var manifestDependencies = new Dictionary<string, PackageDependency>(StringComparer.OrdinalIgnoreCase);
            foreach (var dependency in packageBuilder.DependencyGroups.Where(group => group.TargetFramework == NuGetFramework.AnyFramework)
                                                                      .SelectMany(group => group.Packages))
            {
                manifestDependencies.Add(dependency.Id, dependency);
            }

            updateDependencies = false;
            var workflowDependencies = DependencyInspector.GetWorkflowPackageDependencies(packageBuilder.Files, configuration);
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
