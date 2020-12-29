using Bonsai.NuGet;
using System;
using System.Threading.Tasks;

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

        protected override async Task RunPackageOperationAsync(LicenseAwarePackageManager packageManager, Func<Task> operationFactory)
        {
            await operationFactory().ContinueWith(task =>
            {
                if (task.Exception is AggregateException ex)
                {
                    foreach (var error in ex.InnerExceptions)
                    {
                        packageManager.Logger.LogError(error.Message);
                    }
                }
                else packageManager.Logger.LogError(task.Exception.Message);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
