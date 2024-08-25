using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Bonsai.NuGet
{
    public static class SourceRepositoryExtensions
    {
        public static bool Exists(this SourceRepository repository, PackageIdentity identity, CancellationToken token = default)
        {
            var findPackageResource = repository.GetResource<FindLocalPackagesResource>(token);
            return findPackageResource.Exists(identity, NullLogger.Instance, token);
        }

        public static LocalPackageInfo FindLocalPackage(this SourceRepository repository, string id, CancellationToken token = default)
        {
            var findPackageResource = repository.GetResource<FindLocalPackagesResource>(token);
            var packageInfo = findPackageResource.FindPackagesById(id, NullLogger.Instance, token);
            return packageInfo.FirstOrDefault();
        }

        public static async Task<LocalPackageInfo> FindLocalPackageAsync(this SourceRepository repository, string id, CancellationToken token = default)
        {
            var findPackageResource = await repository.GetResourceAsync<FindLocalPackagesResource>(token);
            var packageInfo = findPackageResource.FindPackagesById(id, NullLogger.Instance, token);
            return packageInfo.FirstOrDefault();
        }

        public static LocalPackageInfo GetLocalPackage(this SourceRepository repository, PackageIdentity identity, CancellationToken token = default)
        {
            var findPackageResource = repository.GetResource<FindLocalPackagesResource>(token);
            return findPackageResource.GetPackage(identity, NullLogger.Instance, token);
        }

        public static IEnumerable<LocalPackageInfo> GetLocalPackages(this SourceRepository repository, CancellationToken token = default)
        {
            var findPackageResource = repository.GetResource<FindLocalPackagesResource>(token);
            return findPackageResource.GetPackages(NullLogger.Instance, token);
        }

        public static async Task<IEnumerable<IPackageSearchMetadata>> SearchAsync(this SourceRepository repository, string searchTerm, SearchFilter filters, int skip, int take, CancellationToken token = default)
        {
            var searchPackageResource = await repository.GetResourceAsync<PackageSearchResource>(token);
            return await searchPackageResource.SearchAsync(searchTerm, filters, skip, take, NullLogger.Instance, token);
        }

        public static async Task<IEnumerable<IPackageSearchMetadata>> GetMetadataAsync(this SourceRepository repository, string id, bool includePrerelease, CancellationToken token = default)
        {
            using var cacheContext = new SourceCacheContext { MaxAge = DateTimeOffset.UtcNow };
            var packageMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>(token);
            return await packageMetadataResource.GetMetadataAsync(id, includePrerelease, includeUnlisted: false, cacheContext, NullLogger.Instance, token);
        }

        public static Task<IEnumerable<IPackageSearchMetadata>> GetUpdatesAsync(this SourceRepository repository, IEnumerable<IPackageSearchMetadata> localPackages, bool includePrerelease, CancellationToken token = default)
        {
            return GetUpdatesAsync(repository, localPackages.Select(package => package.Identity), includePrerelease, token);
        }

        public static Task<IEnumerable<IPackageSearchMetadata>> GetUpdatesAsync(this SourceRepository repository, IEnumerable<LocalPackageInfo> localPackages, bool includePrerelease, CancellationToken token = default)
        {
            return GetUpdatesAsync(repository, localPackages.Select(package => package.Identity), includePrerelease, token);
        }

        public static async Task<IEnumerable<IPackageSearchMetadata>> GetUpdatesAsync(this SourceRepository repository, IEnumerable<PackageIdentity> packages, bool includePrerelease, CancellationToken token = default)
        {
            using var cacheContext = new SourceCacheContext { MaxAge = DateTimeOffset.UtcNow };
            var tasks = packages.Select(package =>
            {
                var updateRange = new VersionRange(package.Version, includeMinVersion: false);
                return GetLatestMetadataAsync(repository, package.Id, updateRange, includePrerelease, cacheContext, token);
            }).ToArray();

            var packageUpdates = await Task.WhenAll(tasks);
            return packageUpdates.Where(package => package != null).ToList();
        }

        public static async Task<IPackageSearchMetadata> GetMetadataAsync(this SourceRepository repository, PackageIdentity identity, SourceCacheContext cacheContext, CancellationToken token = default)
        {
            var packageMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>(token);
            return await packageMetadataResource.GetMetadataAsync(identity, cacheContext, NullLogger.Instance, token);
        }

        public static async Task<IPackageSearchMetadata> GetLatestMetadataAsync(this SourceRepository repository, string id, VersionRange version, bool includePrerelease, SourceCacheContext cacheContext, CancellationToken token = default)
        {
            var packageMetadataResource = await repository.GetResourceAsync<PackageMetadataResource>(token);
            var packageMetadata = await packageMetadataResource.GetMetadataAsync(id, includePrerelease, includeUnlisted: false, cacheContext, NullLogger.Instance, token);
            var packageVersions = packageMetadata
                .Where(package => version.Satisfies(package.Identity.Version))
                .OrderByDescending(package => package.Identity.Version, VersionComparer.VersionRelease)
                .ToArray();
            return packageVersions.Length > 0
                ? PackageSearchMetadataBuilder
                    .FromMetadata(packageVersions[0])
                    .WithVersions(AsyncLazy.New(packageVersions
                    .Select(metadata => new VersionInfo(metadata.Identity.Version, metadata.DownloadCount)
                    {
                        PackageSearchMetadata = metadata
                    }))).Build()
                : null;
        }
    }
}
