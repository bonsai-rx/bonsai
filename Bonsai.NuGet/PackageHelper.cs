using System;
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
using NuGet.Versioning;

namespace Bonsai.NuGet
{
    public static class PackageHelper
    {
        public static readonly string ContentFolder = PathUtility.EnsureTrailingSlash(PackagingConstants.Folders.Content);

        public static bool IsExecutablePackage(PackageIdentity package, NuGetFramework projectFramework, PackageReaderBase packageReader)
        {
            var entryPoint = package.Id + Constants.BonsaiExtension;
            var nearestFrameworkGroup = packageReader.GetContentItems().GetNearest(projectFramework);
            var executablePackage = nearestFrameworkGroup?.Items.Any(file => PathUtility.GetRelativePath(PackageHelper.ContentFolder, file) == entryPoint);
            return executablePackage.GetValueOrDefault();
        }

        public static string InstallExecutablePackage(PackageIdentity package, NuGetFramework projectFramework, PackageReaderBase packageReader, string targetPath)
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

        public static async Task StartInstallPackage(this IPackageManager packageManager, PackageIdentity package, NuGetFramework projectFramework)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            packageManager.Logger.LogInformation(string.Format(Resources.InstallPackageVersion, package.Id, package.Version));
            await packageManager.InstallPackageAsync(package, projectFramework, ignoreDependencies: false, CancellationToken.None);
        }

        public static async Task<PackageReaderBase> StartInstallPackage(this IPackageManager packageManager, string packageId, NuGetVersion version, NuGetFramework projectFramework)
        {
            var logMessage = version == null ? Resources.InstallPackageLatestVersion : Resources.InstallPackageVersion;
            packageManager.Logger.LogInformation(string.Format(logMessage, packageId, version));
            var package = new PackageIdentity(packageId, version);
            return await packageManager.InstallPackageAsync(package, projectFramework, ignoreDependencies: false, CancellationToken.None);
        }

        public static async Task<PackageReaderBase> StartRestorePackage(this IPackageManager packageManager, string packageId, NuGetVersion version, NuGetFramework projectFramework)
        {
            packageManager.Logger.LogInformation(string.Format(Resources.RestorePackageVersion, packageId, version));
            var package = new PackageIdentity(packageId, version);
            return await packageManager.InstallPackageAsync(package, projectFramework, ignoreDependencies: true, CancellationToken.None);
        }
    }
}
