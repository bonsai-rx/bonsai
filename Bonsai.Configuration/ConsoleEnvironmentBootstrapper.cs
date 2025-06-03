using System;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.NuGet;

namespace Bonsai.Configuration
{
    public class ConsoleEnvironmentBootstrapper : IEnvironmentBootstrapper
    {
        public async Task<bool> EnsureBoostrapperExecutableAsync(BootstrapperInfo bootstrapper, CancellationToken cancellationToken = default)
        {
            try
            {
                if (EnvironmentSelector.TryInitializeLocalBootstrapper(bootstrapper) is null)
                    await EnvironmentSelector.DownloadBootstrapperExecutableAsync(
                        bootstrapper,
                        ConsoleLogger.Default,
                        () => new ProgressBar(),
                        cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                ConsoleLogger.Default.LogError(ex.Message);
                return false;
            }
        }
    }
}
