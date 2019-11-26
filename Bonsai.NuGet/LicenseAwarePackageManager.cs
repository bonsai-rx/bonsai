using Bonsai.NuGet.Properties;
using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    public class LicenseAwarePackageManager : PackageManager
    {
        static readonly FrameworkName DefaultFramework = new FrameworkName(".NETFramework,Version=v4.7.2");

        public LicenseAwarePackageManager(IPackageRepository sourceRepository, string path)
            : base(sourceRepository, path)
        {
        }

        public LicenseAwarePackageManager(
            IPackageRepository sourceRepository,
            IPackagePathResolver pathResolver,
            IFileSystem fileSystem)
            : base(sourceRepository, pathResolver, fileSystem)
        {
        }

        public LicenseAwarePackageManager(
            IPackageRepository sourceRepository,
            IPackagePathResolver pathResolver,
            IFileSystem fileSystem,
            IPackageRepository localRepository)
            : base(sourceRepository, pathResolver, fileSystem, localRepository)
        {
        }

        public event EventHandler<RequiringLicenseAcceptanceEventArgs> RequiringLicenseAcceptance;

        protected virtual void OnRequiringLicenseAcceptance(RequiringLicenseAcceptanceEventArgs e)
        {
            var handler = RequiringLicenseAcceptance;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public override void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
            InstallPackage(package, DefaultFramework, ignoreDependencies, allowPrereleaseVersions);
        }

        protected void InstallPackage(
            IPackage package,
            FrameworkName targetFramework,
            bool ignoreDependencies,
            bool allowPrereleaseVersions)
        {
            var installerWalker = new UpdateWalker(
                LocalRepository,
                SourceRepository,
                new DependentsWalker(LocalRepository, targetFramework),
                NullConstraintProvider.Instance,
                targetFramework,
                Logger,
                !ignoreDependencies,
                allowPrereleaseVersions);
            Execute(package, installerWalker);
        }

        private void Execute(IPackage package, IPackageOperationResolver resolver)
        {
            var operations = resolver.ResolveOperations(package).ToList();
            if (operations.Any())
            {
                if (!ShowLicenseAgreement(operations))
                {
                    return;
                }

                foreach (PackageOperation operation in operations)
                {
                    Execute(operation);
                }
            }
            else if (LocalRepository.Exists(package))
            {
                // If the package wasn't installed by our set of operations, notify the user.
                Logger.Log(MessageLevel.Info, Resources.PackageAlreadyInstalled, package.GetFullName());
            }
        }

        private bool ShowLicenseAgreement(IEnumerable<PackageOperation> operations)
        {
            var licensePackages = (from operation in operations
                                   where operation.Action == PackageAction.Install &&
                                         operation.Package.RequireLicenseAcceptance &&
                                         !LocalRepository.Exists(operation.Package)
                                   select operation.Package)
                                   .ToList();

            if (licensePackages.Any())
            {
                var licenseAcceptanceEventArgs = new RequiringLicenseAcceptanceEventArgs(licensePackages);
                OnRequiringLicenseAcceptance(licenseAcceptanceEventArgs);
                return licenseAcceptanceEventArgs.LicenseAccepted;
            }

            return true;
        }
    }
}
