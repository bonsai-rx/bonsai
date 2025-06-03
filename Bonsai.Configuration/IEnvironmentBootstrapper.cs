using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Configuration
{
    public interface IEnvironmentBootstrapper
    {
        Task<bool> EnsureBoostrapperExecutableAsync(
            BootstrapperInfo bootstrapper,
            CancellationToken cancellationToken = default);
    }
}
