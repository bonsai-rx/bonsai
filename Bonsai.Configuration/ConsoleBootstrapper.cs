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
            await operationFactory().ContinueWith(task =>
            {
                if (task.Exception is AggregateException ex)
                {
                    foreach (var error in ex.InnerExceptions)
                    {
                        PackageManager.Logger.LogError(error.Message);
                    }
                }
                else PackageManager.Logger.LogError(task.Exception.Message);
            }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
