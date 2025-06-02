using System;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.NuGet;
using NuGet.Common;

namespace Bonsai.Configuration
{
    public class EnvironmentBootstrapper
    {
        public ILogger Logger { get; set; } = ConsoleLogger.Default;

        protected virtual IProgressBar GetProgressBar() => new ProgressBar();

        protected virtual Task RunBootstrapperOperationAsync(BootstrapperInfo bootstrapper, CancellationToken cancellationToken)
        {
            return EnvironmentSelector.EnsureBootstrapperExecutable(
                bootstrapper,
                Logger,
                GetProgressBar,
                cancellationToken);
        }

        public async Task<bool> RunAsync(BootstrapperInfo bootstrapper, CancellationToken cancellationToken = default)
        {
            try
            {
                await RunBootstrapperOperationAsync(bootstrapper, cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex.Message);
                return false;
            }
        }
    }
}
