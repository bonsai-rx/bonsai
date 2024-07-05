using Bonsai.NuGet;

namespace Bonsai.Configuration
{
    public class ConsoleBootstrapper : Bootstrapper
    {
        public ConsoleBootstrapper(string path)
            : base(path)
        {
            PackageManager.Logger = ConsoleLogger.Default;
            PackageManager.RequiringLicenseAcceptance += (sender, e) => e.LicenseAccepted = true;
        }
    }
}
