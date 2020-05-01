using Bonsai.Configuration;
using Bonsai.NuGet;
using Bonsai.Properties;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using PackageReference = Bonsai.Configuration.PackageReference;
using PackageHelper = Bonsai.NuGet.PackageHelper;

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
            var settings = Settings.LoadDefaultSettings(new PhysicalFileSystem(AppDomain.CurrentDomain.BaseDirectory), null, machineWideSettings);
            var sourceProvider = new PackageSourceProvider(settings);
            var sourceRepository = sourceProvider.CreateAggregateRepository(PackageRepositoryFactory.Default, true);
            return new LicenseAwarePackageManager(sourceRepository, path) { Logger = logger };
        }

        static SemanticVersion ParseVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return null;
            return SemanticVersion.Parse(version);
        }

        static IEnumerable<PackageReference> GetMissingPackages(IEnumerable<PackageReference> packages, IPackageRepository repository)
        {
            return from package in packages
                   let version = ParseVersion(package.Version)
                   where !repository.Exists(package.Id, version)
                   select package;
        }

        internal static IPackage GetEditorPackage(
            PackageConfiguration packageConfiguration,
            string editorRepositoryPath,
            string editorPath,
            IPackageName editorPackageName,
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
                    Task RestoreMissingPackages()
                    {
                        var restoreTasks = missingPackages.Select(package => packageManager.StartRestorePackage(package.Id, ParseVersion(package.Version)));
                        return Task.Factory.ContinueWhenAll(restoreTasks.ToArray(), operations =>
                        {
                            foreach (var task in operations)
                            {
                                if (task.IsFaulted || task.IsCanceled) continue;
                                var package = task.Result;
                                if (packageManager.LocalRepository.Exists(package.Id))
                                {
                                    packageManager.UpdatePackage(
                                        package,
                                        updateDependencies: false,
                                        allowPrereleaseVersions: true);
                                }
                                else
                                {
                                    packageManager.InstallPackage(
                                        package,
                                        ignoreDependencies: true,
                                        allowPrereleaseVersions: true,
                                        ignoreWalkInfo: true);
                                }
                            }

                            Task.WaitAll(operations);
                        });
                    };

                    if (!showDialog) RestoreMissingPackages().Wait();
                    else PackageHelper.RunPackageOperation(packageManager, RestoreMissingPackages);
                }
            }

            var editorPackage = packageManager.LocalRepository.FindPackage(editorPackageName.Id);
            if (editorPackage == null || editorPackage.Version < editorPackageName.Version)
            {
                if (showDialog) EnableVisualStyles();
                using (var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, editorPath, editorPackageName))
                {
                    Task RestoreEditorPackage()
                    {
                        return packageManager
                            .StartInstallPackage(editorPackageName.Id, editorPackageName.Version)
                            .ContinueWith(task => editorPackage = task.Result);
                    };

                    if (!showDialog) RestoreEditorPackage().Wait();
                    else PackageHelper.RunPackageOperation(
                        packageManager,
                        RestoreEditorPackage,
                        operationLabel: editorPackage != null ? "Updating..." : null);
                    if (editorPackage == null)
                    {
                        var assemblyName = Assembly.GetEntryAssembly().GetName();
                        var errorMessage = editorPackage == null ? Resources.InstallEditorPackageError : Resources.UpdateEditorPackageError;
                        if (!showDialog) ConsoleLogger.Default.Log(MessageLevel.Error, errorMessage);
                        else MessageBox.Show(errorMessage, assemblyName.Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
            }

            return editorPackage;
        }
    }
}
