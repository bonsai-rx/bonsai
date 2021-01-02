using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    public interface IPackageManager
    {
        ILogger Logger { get; set; }

        DependencyBehavior DependencyBehavior { get; set; }

        PackagePathResolver PathResolver { get; }

        SourceRepository LocalRepository { get; }

        ISourceRepositoryProvider SourceRepositoryProvider { get; }

        ISettings Settings { get; }

        ICollection<PackageManagerPlugin> PackageManagerPlugins { get; }

        Task<IEnumerable<LocalPackageInfo>> GetInstalledPackagesAsync(CancellationToken token);

        Task<PackageReaderBase> InstallPackageAsync(PackageIdentity package, NuGetFramework projectFramework, bool ignoreDependencies, CancellationToken token);

        Task<bool> UninstallPackageAsync(PackageIdentity package, NuGetFramework projectFramework, bool removeDependencies, CancellationToken token);
    }
}
