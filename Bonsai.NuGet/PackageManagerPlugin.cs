using NuGet.Packaging;
using NuGet.Packaging.Core;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    public class PackageManagerPlugin
    {
        public virtual Task<bool> OnPackageInstallingAsync(PackageIdentity package, PackageReaderBase packageReader, string installPath)
        {
            return Task.FromResult(true);
        }

        public virtual Task OnPackageInstalledAsync(PackageIdentity package, PackageReaderBase packageReader, string installPath)
        {
            return Task.CompletedTask;
        }

        public virtual Task<bool> OnPackageUninstallingAsync(PackageIdentity package, PackageReaderBase packageReader, string installPath)
        {
            return Task.FromResult(true);
        }

        public virtual Task OnPackageUninstalledAsync(PackageIdentity package, PackageReaderBase packageReader, string installPath)
        {
            return Task.CompletedTask;
        }
    }
}
