using NuGet.Packaging.Core;

namespace Bonsai.NuGet
{
    public sealed class PackageDependencyConflict
    {
        public PackageDependencyConflict(PackageIdentity package, PackageDependency dependency, PackageIdentity resolvedPackage)
        {
            Package = package;
            Dependency = dependency;
            ResolvedPackage = resolvedPackage;
        }

        public PackageIdentity Package { get; }

        public PackageDependency Dependency { get; }

        public PackageIdentity ResolvedPackage { get; }

        public override string ToString()
        {
            return $"'{Package.Id} {Package.Version}' requires '{Dependency.Id} {Dependency.VersionRange.PrettyPrint()}' " +
                   $"but '{ResolvedPackage.Id} {ResolvedPackage.Version}' would be installed";
        }
    }
}
