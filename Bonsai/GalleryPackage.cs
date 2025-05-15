using Bonsai.Configuration;
using Bonsai.NuGet.Packaging;
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
using System.Text.RegularExpressions;

namespace Bonsai
{
    static class GalleryPackage
    {
        const RegexOptions DefaultRegexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;
        static readonly Regex LicenseFiles = new("LICENSE(.md|.txt|.rst)?", DefaultRegexOptions);
        static readonly Regex ReadmeFiles = new("README(.md)?", DefaultRegexOptions);
        static readonly Regex IconFiles = new("icon(.png|.jpg)", DefaultRegexOptions);
        static readonly string ExcludeFiles =
            $@"**\*{NuGetConstants.ManifestExtension};" +
            $@"**\*{NuGetConstants.PackageExtension};" +
            $@"**\{NuGet.Constants.BonsaiExtension}\**";

        public static Manifest OpenManifest(string metadataPath)
        {
            using var stream = File.OpenRead(metadataPath);
            return Manifest.ReadFrom(stream, true);
        }

        public static Manifest CreateDefaultManifest(string metadataPath)
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

        static BonsaiMetadataPackageFile GetBonsaiMetadataFile(string path)
        {
            var bonsaiMetadata = new BonsaiMetadata();
            bonsaiMetadata.Gallery[BonsaiMetadata.DefaultWorkflow] = new() { Path = path };
            return new BonsaiMetadataPackageFile(bonsaiMetadata);
        }

        public static PackageBuilder CreatePackageBuilder(string path, Manifest manifest, PackageConfiguration configuration)
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
            packageBuilder.Repository = RepositoryUtility.GetRepositoryMetadata(basePath);
            if (packageBuilder.LicenseMetadata is not null)
                packageBuilder.LicenseUrl = null;

            if (manifest.HasFilesNode)
                packageBuilder.PopulateFiles(basePath, manifest.Files);
            else
                packageBuilder.AddFiles(basePath, "**", PackagingConstants.Folders.Content, ExcludeFiles);

            PhysicalPackageFile entryPointFile = default;
            foreach (var file in packageBuilder.Files)
            {
                if (file is PhysicalPackageFile packageFile)
                {
                    var packageFileName = Path.GetFileName(packageFile.EffectivePath);
                    if (packageBuilder.LicenseMetadata is null && packageBuilder.LicenseUrl is null &&
                        LicenseFiles.IsMatch(packageFileName))
                    {
                        packageBuilder.LicenseMetadata = new LicenseMetadata(
                            LicenseType.File,
                            packageFileName,
                            expression: default,
                            null,
                            LicenseMetadata.CurrentVersion);
                        packageFile.TargetPath = packageFileName;
                    }

                    if (string.IsNullOrEmpty(packageBuilder.Readme) && ReadmeFiles.IsMatch(packageFileName))
                    {
                        packageBuilder.Readme = packageFileName;
                        packageFile.TargetPath = packageFileName;
                    }

                    if (string.IsNullOrEmpty(packageBuilder.Icon) && packageBuilder.IconUrl is null &&
                        IconFiles.IsMatch(packageFileName))
                    {
                        packageBuilder.Icon = packageFileName;
                        packageFile.TargetPath = packageFileName;
                    }

                    if (packageFile.SourcePath == path)
                        entryPointFile = packageFile;
                }
            }

            if (entryPointFile is not null)
            {
                var bonsaiMetadata = GetBonsaiMetadataFile(entryPointFile.EffectivePath);
                packageBuilder.Files.Add(bonsaiMetadata);
            }

            var manifestDependencies = new Dictionary<string, PackageDependency>(StringComparer.OrdinalIgnoreCase);
            foreach (var dependency in packageBuilder.DependencyGroups.Where(group => group.TargetFramework == NuGetFramework.AnyFramework)
                                                                      .SelectMany(group => group.Packages))
            {
                manifestDependencies.Add(dependency.Id, dependency);
            }

            var workflowDependencies = DependencyInspector.GetWorkflowPackageDependencies(packageBuilder.Files, configuration);
            foreach (var dependency in workflowDependencies)
            {
                if (!manifestDependencies.TryGetValue(dependency.Id, out PackageDependency manifestDependency) ||
                    !DependencyEqualityComparer.Default.Equals(dependency, manifestDependency))
                {
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
