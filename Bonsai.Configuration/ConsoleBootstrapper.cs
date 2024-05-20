using Bonsai.NuGet;
using System;
using System.Threading.Tasks;

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

        protected override async Task RunPackageOperationAsync(Func<Task> operationFactory)
        {
            try { await operationFactory(); }
            catch (Exception ex)
            {
                PackageManager.Logger.LogError(ex.ToString());
                throw;
            }
        }
    }
}
