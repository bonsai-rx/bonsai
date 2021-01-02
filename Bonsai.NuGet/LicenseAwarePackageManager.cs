using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;

namespace Bonsai.NuGet
{
    public class LicenseAwarePackageManager : PackageManager
    {
        public LicenseAwarePackageManager(PackageSourceProvider packageSourceProvider, string path)
            : this(packageSourceProvider.Settings, packageSourceProvider, path)
        {
        }

        public LicenseAwarePackageManager(ISettings settings, IPackageSourceProvider packageSourceProvider, string path)
            : this(settings, packageSourceProvider, new PackageSource(path))
        {
        }

        public LicenseAwarePackageManager(ISettings settings, IPackageSourceProvider packageSourceProvider, PackageSource localRepository)
            : base(settings, packageSourceProvider, localRepository)
        {
        }

        public event EventHandler<RequiringLicenseAcceptanceEventArgs> RequiringLicenseAcceptance;

        protected override bool AcceptLicenseAgreement(IEnumerable<IPackageSearchMetadata> licensePackages)
        {
            var eventArgs = new RequiringLicenseAcceptanceEventArgs(licensePackages);
            RequiringLicenseAcceptance?.Invoke(this, eventArgs);
            return eventArgs.LicenseAccepted;
        }
    }
}
