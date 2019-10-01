using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Bonsai.Design;
using Bonsai.Expressions;
using NuGet;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bonsai.NuGet.Properties;

namespace Bonsai.NuGet
{
    public static class PackageHelper
    {
        public static string InstallExecutablePackage(IPackage package, IFileSystem fileSystem)
        {
            var targetId = Path.GetFileName(fileSystem.Root);
            var targetEntryPoint = targetId + Constants.BonsaiExtension;
            var targetEntryPointLayout = targetEntryPoint + Constants.LayoutExtension;
            var packageEntryPoint = package.Id + Constants.BonsaiExtension;
            var packageEntryPointLayout = packageEntryPoint + Constants.LayoutExtension;

            foreach (var file in package.GetContentFiles())
            {
                var effectivePath = file.EffectivePath;
                if (effectivePath == packageEntryPoint) effectivePath = targetEntryPoint;
                else if (effectivePath == packageEntryPointLayout) effectivePath = targetEntryPointLayout;

                using (var stream = file.GetStream())
                {
                    fileSystem.AddFile(effectivePath, stream);
                }
            }

            var manifest = Manifest.Create(package);
            var metadata = Manifest.Create(manifest.Metadata);
            var metadataPath = targetId + global::NuGet.Constants.ManifestExtension;
            using (var stream = fileSystem.CreateFile(metadataPath))
            {
                metadata.Save(stream);
            }

            return fileSystem.GetFullPath(targetEntryPoint);
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
            else logger.Log(MessageLevel.Error, exception.Message);
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

        public static Task StartInstallPackage(this IPackageManager packageManager, IPackage package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            return Task.Factory.StartNew(() =>
            {
                packageManager.Logger.Log(MessageLevel.Info, Resources.InstallPackageVersion, package.Id, package.Version);
                packageManager.InstallPackage(package, false, true);
            });
        }

        public static Task<IPackage> StartInstallPackage(this IPackageManager packageManager, string packageId, SemanticVersion version)
        {
            return Task.Factory.StartNew(() =>
            {
                var logMessage = version == null ? Resources.InstallPackageLatestVersion : Resources.InstallPackageVersion;
                packageManager.Logger.Log(MessageLevel.Info, logMessage, packageId, version);
                var package = packageManager.SourceRepository.FindPackage(packageId, version);
                if (package == null)
                {
                    var errorMessage = version == null ? Resources.MissingPackageLatestVersion : Resources.MissingPackageVersion;
                    throw new InvalidOperationException(string.Format(errorMessage, packageId, version));
                }
                packageManager.InstallPackage(package, false, true);
                return packageManager.LocalRepository.FindPackage(packageId, version);
            });
        }

        public static Task<IPackage> StartRestorePackage(this IPackageManager packageManager, string packageId, SemanticVersion version)
        {
            return Task.Factory.StartNew(() =>
            {
                packageManager.Logger.Log(MessageLevel.Info, Resources.RestorePackageVersion, packageId, version);
                var package = packageManager.SourceRepository.FindPackage(packageId, version);
                if (package == null)
                {
                    var errorMessage = string.Format(Resources.MissingPackageVersion, packageId, version);
                    throw new InvalidOperationException(errorMessage);
                }
                return package;
            });
        }
    }
}
