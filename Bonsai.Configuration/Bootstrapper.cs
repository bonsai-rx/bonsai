using Bonsai.NuGet;
using NuGet.Configuration;
using NuGet.Versioning;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bonsai.Configuration
{
    public class Bootstrapper
    {
        public virtual LicenseAwarePackageManager CreatePackageManager(string path)
        {
            var machineWideSettings = new BonsaiMachineWideSettings();
            var settings = Settings.LoadDefaultSettings(AppDomain.CurrentDomain.BaseDirectory, null, machineWideSettings);
            var sourceProvider = new PackageSourceProvider(settings);
            return new LicenseAwarePackageManager(sourceProvider, path);
        }

        protected virtual async Task RunPackageOperationAsync(LicenseAwarePackageManager packageManager, Func<Task> operationFactory)
        {
            await operationFactory();
        }

        static NuGetVersion ParseVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return null;
            return NuGetVersion.Parse(version);
        }

        static IEnumerable<PackageReference> GetMissingPackages(PackageConfiguration packageConfiguration, SourceRepository repository)
        {
            return from package in packageConfiguration.Packages
                   let version = ParseVersion(package.Version)
                   where !repository.Exists(new PackageIdentity(package.Id, version))
                   select package;
        }

        public async Task RunAsync(
            PackageConfiguration packageConfiguration,
            LicenseAwarePackageManager packageManager,
            string bootstrapperPath,
            PackageIdentity bootstrapperPackage)
        {
            const string OldExtension = ".old";
            var backupExePath = bootstrapperPath + OldExtension;
            if (File.Exists(backupExePath))
            {
                try { File.Delete(backupExePath); }
                catch { } // best effort
            }

            var missingPackages = GetMissingPackages(packageConfiguration, packageManager.LocalRepository).ToList();
            if (missingPackages.Count > 0)
            {
                async Task RestoreMissingPackages()
                {
                    using var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, bootstrapperPath, bootstrapperPackage);
                    foreach (var package in missingPackages)
                    {
                        await packageManager.StartRestorePackage(package.Id, ParseVersion(package.Version));
                    }
                };

                await RunPackageOperationAsync(packageManager, RestoreMissingPackages);
            }

            var editorPackage = packageManager.LocalRepository.FindLocalPackage(bootstrapperPackage.Id);
            if (editorPackage == null || editorPackage.Identity.Version < bootstrapperPackage.Version)
            {
                async Task RestoreEditorPackage()
                {
                    using var monitor = new PackageConfigurationUpdater(packageConfiguration, packageManager, bootstrapperPath, bootstrapperPackage);
                    var package = await packageManager.StartInstallPackage(bootstrapperPackage.Id, bootstrapperPackage.Version);
                    editorPackage = packageManager.LocalRepository.GetLocalPackage(package.GetIdentity());
                };

                await RunPackageOperationAsync(packageManager, RestoreEditorPackage);
            }
        }
    }
}
