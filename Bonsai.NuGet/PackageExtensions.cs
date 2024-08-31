using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.NuGet.Properties;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Versioning;

namespace Bonsai.NuGet
{
    public static class PackageExtensions
    {
        const string PackageTagFilter = "Bonsai";
        public static readonly string ContentFolder = PathUtility.EnsureTrailingSlash(PackagingConstants.Folders.Content);

        public static bool IsPackageType(this LocalPackageInfo packageInfo, string typeName)
        {
            return packageInfo.Nuspec.IsPackageType(typeName);
        }

        public static bool IsPackageType(this PackageReaderBase packageReader, string typeName)
        {
            return packageReader.NuspecReader.IsPackageType(typeName);
        }

        public static bool IsPackageType(this NuspecReader reader, string typeName)
        {
            return reader.GetPackageTypes().IsPackageType(typeName);
        }

        public static bool IsPackageType(this IReadOnlyList<PackageType> packageTypes, string typeName)
        {
            if (packageTypes.Count == 0
                && PackageType.PackageTypeNameComparer.Equals(typeName, PackageType.Dependency.Name))
            {
                return true;
            }

            return packageTypes.Any(type => PackageType.PackageTypeNameComparer.Equals(type.Name, typeName));
        }

        public static bool IsLibraryPackage(this PackageReaderBase packageReader)
        {
            return packageReader.IsPackageType(Constants.LibraryPackageType)
                || packageReader.NuspecReader.GetTags()?.Contains(PackageTagFilter) is true;
        }

        public static bool IsGalleryPackage(this PackageReaderBase packageReader)
        {
            return packageReader.IsPackageType(Constants.GalleryPackageType)
                || packageReader.NuspecReader.GetTags()?.Contains(PackageTagFilter) is true;
        }

        public static bool IsExecutablePackage(this PackageReaderBase packageReader, PackageIdentity identity, NuGetFramework projectFramework)
        {
            var entryPoint = identity.Id + Constants.BonsaiExtension;
            var nearestFrameworkGroup = packageReader.GetContentItems().GetNearest(projectFramework);
            var executablePackage = nearestFrameworkGroup?.Items.Any(file => PathUtility.GetRelativePath(ContentFolder, file) == entryPoint);
            return IsGalleryPackage(packageReader) && executablePackage.GetValueOrDefault();
        }

        public static string InstallExecutablePackage(this PackageReaderBase packageReader, PackageIdentity package, NuGetFramework projectFramework, string targetPath)
        {
            var targetId = Path.GetFileName(targetPath);
            var targetEntryPoint = targetId + Constants.BonsaiExtension;
            var targetEntryPointLayout = targetEntryPoint + Constants.LayoutExtension;
            var packageEntryPoint = package.Id + Constants.BonsaiExtension;
            var packageEntryPointLayout = packageEntryPoint + Constants.LayoutExtension;

            var nearestFrameworkGroup = packageReader.GetContentItems().GetNearest(projectFramework);
            if (nearestFrameworkGroup != null)
            {
                foreach (var file in nearestFrameworkGroup.Items)
                {
                    var effectivePath = PathUtility.GetRelativePath(ContentFolder, file);
                    if (effectivePath == packageEntryPoint) effectivePath = targetEntryPoint;
                    else if (effectivePath == packageEntryPointLayout) effectivePath = targetEntryPointLayout;
                    effectivePath = Path.Combine(targetPath, effectivePath);
                    PathUtility.EnsureParentDirectory(effectivePath);

                    using (var stream = packageReader.GetStream(file))
                    using (var targetStream = File.Create(effectivePath))
                    {
                        stream.CopyTo(targetStream);
                    }
                }
            }

            var effectiveEntryPoint = Path.Combine(targetPath, targetEntryPoint);
            if (!File.Exists(effectiveEntryPoint))
            {
                var message = string.Format(Resources.MissingWorkflowEntryPoint, targetEntryPoint);
                throw new InvalidOperationException(message);
            }

            var manifestFile = packageReader.GetNuspecFile();
            var metadataPath = Path.Combine(targetPath, targetId + NuGetConstants.ManifestExtension);
            using (var manifestStream = packageReader.GetStream(manifestFile))
            using (var manifestTargetStream = File.Create(metadataPath))
            {
                var manifest = Manifest.ReadFrom(manifestStream, validateSchema: true);
                manifest.Save(manifestTargetStream);
            }

            return effectiveEntryPoint;
        }

        public static async Task<PackageReaderBase> InstallPackageAsync(
            this IPackageManager packageManager,
            string packageId,
            NuGetVersion version,
            NuGetFramework projectFramework,
            CancellationToken cancellationToken = default)
        {
            var package = new PackageIdentity(packageId, version);
            var logMessage = package.Version == null ? Resources.InstallPackageLatestVersion : Resources.InstallPackageVersion;
            packageManager.Logger.LogInformation(string.Format(logMessage, package.Id, package.Version));
            return await packageManager.InstallPackageAsync(package, projectFramework, ignoreDependencies: false, cancellationToken);
        }

        public static async Task<PackageReaderBase> RestorePackageAsync(
            this IPackageManager packageManager,
            string packageId,
            NuGetVersion version,
            NuGetFramework projectFramework,
            CancellationToken cancellationToken = default)
        {
            var package = new PackageIdentity(packageId, version);
            packageManager.Logger.LogInformation(string.Format(Resources.RestorePackageVersion, packageId, version));
            return await packageManager.InstallPackageAsync(package, projectFramework, ignoreDependencies: true, cancellationToken);
        }
    }
}
