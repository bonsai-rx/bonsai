using System;
using System.Collections.Generic;

namespace Bonsai.NuGet
{
    public class RequiringLicenseAcceptanceEventArgs : EventArgs
    {
        public RequiringLicenseAcceptanceEventArgs(IEnumerable<RequiringLicenseAcceptancePackageInfo> licensePackages)
        {
            LicensePackages = licensePackages ?? throw new ArgumentNullException(nameof(licensePackages));
        }

        public IEnumerable<RequiringLicenseAcceptancePackageInfo> LicensePackages { get; private set; }

        public bool LicenseAccepted { get; set; }
    }
}
