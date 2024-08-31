using Bonsai.NuGet;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Configuration
{
    public class Bootstrapper
    {
        readonly RestorePackageManager packageManager;

        public Bootstrapper(string path)
        {
            var machineWideSettings = new BonsaiMachineWideSettings();
            var settings = Settings.LoadDefaultSettings(AppDomain.CurrentDomain.BaseDirectory, null, machineWideSettings);
            var sourceProvider = new PackageSourceProvider(settings);
            packageManager = new RestorePackageManager(sourceProvider, path);
        }

        public LicenseAwarePackageManager PackageManager
        {
            get { return packageManager; }
        }

        protected virtual async Task RunPackageOperationAsync(Func<CancellationToken, Task> operationFactory, CancellationToken cancellationToken)
        {
            await operationFactory(cancellationToken);
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
            NuGetFramework projectFramework,
            PackageConfiguration packageConfiguration,
            string bootstrapperPath,
            PackageIdentity bootstrapperPackage,
            CancellationToken cancellationToken = default)
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
                async Task RestoreMissingPackages(CancellationToken cancellationToken)
                {
                    using var monitor = new PackageConfigurationUpdater(
                        projectFramework,
                        packageConfiguration,
                        packageManager,
                        bootstrapperPath,
                        bootstrapperPackage);
                    foreach (var package in missingPackages)
                    {
                        await packageManager.RestorePackageAsync(
                            package.Id,
                            ParseVersion(package.Version),
                            projectFramework,
                            cancellationToken);
                    }
                };

                packageManager.RestorePackages = true;
                try { await RunPackageOperationAsync(RestoreMissingPackages, cancellationToken); }
                finally { packageManager.RestorePackages = false; }
            }

            var editorPackage = packageManager.LocalRepository.FindLocalPackage(bootstrapperPackage.Id);
            if (editorPackage == null || editorPackage.Identity.Version < bootstrapperPackage.Version)
            {
                async Task RestoreEditorPackage(CancellationToken cancellationToken)
                {
                    using var monitor = new PackageConfigurationUpdater(
                        projectFramework,
                        packageConfiguration,
                        packageManager,
                        bootstrapperPath,
                        bootstrapperPackage);
                    var package = await packageManager.InstallPackageAsync(
                        bootstrapperPackage.Id,
                        bootstrapperPackage.Version,
                        projectFramework,
                        cancellationToken);
                    editorPackage = packageManager.LocalRepository.GetLocalPackage(package.GetIdentity());
                };

                await RunPackageOperationAsync(RestoreEditorPackage, cancellationToken);
            }
        }

        class RestorePackageManager : LicenseAwarePackageManager
        {
            public RestorePackageManager(PackageSourceProvider packageSourceProvider, string path)
                : base(packageSourceProvider.Settings, packageSourceProvider, path)
            {
            }

            public bool RestorePackages { get; set; }

            protected override bool AcceptLicenseAgreement(IEnumerable<RequiringLicenseAcceptancePackageInfo> licensePackages)
            {
                if (RestorePackages) return true;
                else return base.AcceptLicenseAgreement(licensePackages);
            }
        }
    }
}
