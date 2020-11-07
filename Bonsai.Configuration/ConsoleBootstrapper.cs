using Bonsai.NuGet;

namespace Bonsai.Configuration
{
    public class ConsoleBootstrapper : Bootstrapper
    {
        public static readonly Bootstrapper Default = new ConsoleBootstrapper();

        public override LicenseAwarePackageManager CreatePackageManager(string path)
        {
            var packageManager = base.CreatePackageManager(path);
            packageManager.Logger = ConsoleLogger.Default;
            packageManager.RequiringLicenseAcceptance += (sender, e) => e.LicenseAccepted = true;
            return packageManager;
        }
    }
}
