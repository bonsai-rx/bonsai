using NuGet;
using System;
using System.Collections.Generic;

namespace Bonsai.NuGet
{
    public class RequiringLicenseAcceptanceEventArgs : EventArgs
    {
        public RequiringLicenseAcceptanceEventArgs(IEnumerable<IPackage> licensePackages)
        {
            if (licensePackages == null)
            {
                throw new ArgumentNullException("licensePackages");
            }

            LicensePackages = licensePackages;
        }

        public IEnumerable<IPackage> LicensePackages { get; private set; }

        public bool LicenseAccepted { get; set; }
    }
}
