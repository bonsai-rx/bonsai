using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;

namespace Bonsai.NuGet
{
    public class RequiringLicenseAcceptanceEventArgs : EventArgs
    {
        public RequiringLicenseAcceptanceEventArgs(IEnumerable<IPackageSearchMetadata> licensePackages)
        {
            LicensePackages = licensePackages ?? throw new ArgumentNullException(nameof(licensePackages));
        }

        public IEnumerable<IPackageSearchMetadata> LicensePackages { get; private set; }

        public bool LicenseAccepted { get; set; }
    }
}
