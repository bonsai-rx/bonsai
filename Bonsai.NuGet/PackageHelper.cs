using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        static void LogException(ILogger logger, Exception exception)
        {
            var aggregateException = exception as AggregateException;
            if (aggregateException != null)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    LogException(logger, innerException);
                }
            }
            else logger.Log(LogLevel.Error, exception.Message);
        }

        public static void RunPackageOperation(LicenseAwarePackageManager packageManager, Func<Task> operationFactory, string operationLabel = null)
        {
            EventHandler<RequiringLicenseAcceptanceEventArgs> requiringLicenseHandler = null;
            using (var dialog = new PackageOperationDialog { ShowInTaskbar = true })
            {
                if (!string.IsNullOrEmpty(operationLabel)) dialog.Text = operationLabel;
                requiringLicenseHandler = (sender, e) =>
                {
                    if (dialog.InvokeRequired) dialog.Invoke(requiringLicenseHandler, sender, e);
                    else
                    {
                        dialog.Hide();
                        using (var licenseDialog = new LicenseAcceptanceDialog(e.LicensePackages))
                        {
                            e.LicenseAccepted = licenseDialog.ShowDialog() == DialogResult.Yes;
                            if (e.LicenseAccepted)
                            {
                                dialog.Show();
                            }
                        }
                    }
                };

                dialog.RegisterEventLogger((EventLogger)packageManager.Logger);
                packageManager.RequiringLicenseAcceptance += requiringLicenseHandler;
                try
                {
                    var operation = operationFactory();
                    operation.ContinueWith(task =>
                    {
                        if (task.IsFaulted) LogException(packageManager.Logger, task.Exception);
                        else dialog.BeginInvoke((Action)dialog.Close);
                    });

                    dialog.ShowDialog();
                }
                finally { packageManager.RequiringLicenseAcceptance -= requiringLicenseHandler; }
            }
        }

        public static async Task StartInstallPackage(this IPackageManager packageManager, PackageIdentity package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            packageManager.Logger.LogInformation(string.Format(Resources.InstallPackageVersion, package.Id, package.Version));
            await packageManager.InstallPackageAsync(package, ignoreDependencies: false, CancellationToken.None);
        }

        public static async Task<PackageReaderBase> StartInstallPackage(this IPackageManager packageManager, string packageId, NuGetVersion version)
        {
            var logMessage = version == null ? Resources.InstallPackageLatestVersion : Resources.InstallPackageVersion;
            packageManager.Logger.LogInformation(string.Format(logMessage, packageId, version));
            var package = new PackageIdentity(packageId, version);
            return await packageManager.InstallPackageAsync(package, ignoreDependencies: false, CancellationToken.None);
        }

        public static async Task<PackageReaderBase> StartRestorePackage(this IPackageManager packageManager, string packageId, NuGetVersion version)
        {
            packageManager.Logger.LogInformation(string.Format(Resources.RestorePackageVersion, packageId, version));
            var package = new PackageIdentity(packageId, version);
            return await packageManager.InstallPackageAsync(package, ignoreDependencies: true, CancellationToken.None);
        }
    }
}
