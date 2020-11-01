using Bonsai.Configuration;
using Bonsai.NuGet;
using Bonsai.Properties;
using NuGet.Configuration;
using NuGet.Versioning;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bonsai.NuGet.Design;

namespace Bonsai
{
    class Bootstrapper
    {
        static bool visualStylesEnabled;

        protected static void EnableVisualStyles()
        {
            if (!visualStylesEnabled)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                visualStylesEnabled = true;
            }
        }

        protected static LicenseAwarePackageManager CreatePackageManager(string path)
        {
            var logger = new EventLogger();
            var machineWideSettings = new BonsaiMachineWideSettings();
            var settings = Settings.LoadDefaultSettings(AppDomain.CurrentDomain.BaseDirectory, null, machineWideSettings);
            var sourceProvider = new PackageSourceProvider(settings);
            return new LicenseAwarePackageManager(sourceProvider, path) { Logger = logger };
        }

        static NuGetVersion ParseVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return null;
            return NuGetVersion.Parse(version);
        }

        static IEnumerable<PackageReference> GetMissingPackages(IEnumerable<PackageReference> packages, SourceRepository repository)
        {
            return from package in packages
                   let version = ParseVersion(package.Version)
                   where !repository.Exists(new PackageIdentity(package.Id, version))
                   select package;
        }

        internal static LocalPackageInfo GetEditorPackage(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            PackageIdentity editorPackageName,
            bool showDialog)
        {
            const string OldExtension = ".old";
            var backupExePath = editorPath + OldExtension;
            if (File.Exists(backupExePath))
            {
                try { File.Delete(backupExePath); }
                catch { } // best effort
            }

            var packageManager = CreatePackageManager(editorRepositoryPath);
            if (!showDialog)
            {
                packageManager.Logger = ConsoleLogger.Default;
                packageManager.RequiringLicenseAcceptance += (sender, e) => e.LicenseAccepted = true;
            }

            var missingPackages = GetMissingPackages(packageConfiguration.Packages, packageManager.LocalRepository).ToList();
            if (missingPackages.Count > 0)
            {
                if (showDialog) EnableVisualStyles();
                using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageName))
                {
                    async Task RestoreMissingPackages()
                    {
                        foreach (var package in missingPackages)
                        {
                            await packageManager.StartRestorePackage(package.Id, ParseVersion(package.Version));
                        }
                    };

                    if (!showDialog) RestoreMissingPackages().Wait();
                    else PackageOperation.Run(packageManager, RestoreMissingPackages);
                }
            }

            var editorPackage = packageManager.LocalRepository.FindLocalPackage(editorPackageName.Id);
            if (editorPackage == null || editorPackage.Identity.Version < editorPackageName.Version)
            {
                if (showDialog) EnableVisualStyles();
                using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageName))
                {
                    Task RestoreEditorPackage()
                    {
                        return packageManager
                            .StartInstallPackage(editorPackageName.Id, editorPackageName.Version)
                            .ContinueWith(task => editorPackage = packageManager.LocalRepository.GetLocalPackage(task.Result.GetIdentity()));
                    };

                    if (!showDialog) RestoreEditorPackage().Wait();
                    else PackageOperation.Run(
                        packageManager,
                        RestoreEditorPackage,
                        operationLabel: editorPackage != null ? "Updating..." : null);
                    if (editorPackage == null)
                    {
                        var assemblyName = Assembly.GetEntryAssembly().GetName();
                        var errorMessage = editorPackage == null ? Resources.InstallEditorPackageError : Resources.UpdateEditorPackageError;
                        if (!showDialog) ConsoleLogger.Default.LogError(errorMessage);
                        else MessageBox.Show(errorMessage, assemblyName.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
            }

            return editorPackage;
        }
    }
}
