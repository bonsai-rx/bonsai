using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

namespace Bonsai.Configuration
{
    public static class ScriptExtensionsProvider
    {
        const string ReferenceElement = "Reference";
        const string IncludeAttribute = "Include";
        const string PropertyExtension = ".props";
        const string DllExtension = ".dll";

        static IEnumerable<string> FindAssemblyReferences(
            NuGetFramework projectFramework,
            DependencyInfoResource dependencyResource,
            FindLocalPackagesResource localPackageResource,
            SourceCacheContext cacheContext,
            string packageId)
        {
            var packageInfo = localPackageResource.FindPackagesById(packageId, NullLogger.Instance, CancellationToken.None).FirstOrDefault();
            if (packageInfo == null) yield break;
            using (var reader = packageInfo.GetReader())
            {
                var assemblyReferences = reader.GetReferenceItems().GetNearest(projectFramework);
                if (assemblyReferences != null)
                {
                    foreach (var assembly in assemblyReferences.Items)
                    {
                        yield return Path.GetFileName(assembly);
                    }
                }

                var buildReferences = reader.GetBuildItems().GetNearest(projectFramework);
                if (buildReferences != null)
                {
                    var buildProperties = buildReferences.Items.FirstOrDefault(item => Path.GetExtension(item) == PropertyExtension);
                    if (buildProperties != null)
                    {
                        using (var propertyStream = reader.GetStream(buildProperties))
                        using (var propertyReader = XmlReader.Create(propertyStream))
                        {
                            while (propertyReader.ReadToFollowing(ReferenceElement))
                            {
                                var assemblyName = propertyReader.GetAttribute(IncludeAttribute);
                                yield return assemblyName + DllExtension;
                            }
                        }
                    }
                }
            }

            var dependencyInfo = dependencyResource.ResolvePackage(packageInfo.Identity, projectFramework, cacheContext, NullLogger.Instance, CancellationToken.None).Result;
            foreach (var dependency in dependencyInfo.Dependencies)
            {
                foreach (var reference in FindAssemblyReferences(projectFramework, dependencyResource, localPackageResource, cacheContext, dependency.Id))
                {
                    yield return reference;
                }
            }
        }

        public static IEnumerable<string> GetPackageAssemblyLocations(
            NuGetFramework projectFramework,
            PackageConfiguration configuration,
            string editorRepositoryPath,
            IEnumerable<string> packageIds)
        {
            var packageSource = new PackageSource(editorRepositoryPath);
            var packageRepository = new SourceRepository(packageSource, Repository.Provider.GetCoreV3());
            var dependencyResource = packageRepository.GetResource<DependencyInfoResource>();
            var localPackageResource = packageRepository.GetResource<FindLocalPackagesResource>();
            HashSet<string> assemblyNames;
            using (var cacheContext = new SourceCacheContext())
            {
                assemblyNames = new HashSet<string>(
                    from id in packageIds
                    from assemblyReference in FindAssemblyReferences(
                        projectFramework,
                        dependencyResource,
                        localPackageResource,
                        cacheContext,
                        packageId: id)
                    select Path.GetFileNameWithoutExtension(assemblyReference),
                    StringComparer.OrdinalIgnoreCase);
            }

            var configurationRoot = ConfigurationHelper.GetConfigurationRoot(configuration);
            foreach (var assemblyName in assemblyNames)
            {
                var assemblyLocation = ConfigurationHelper.GetAssemblyLocation(configuration, assemblyName);
                if (assemblyLocation is null)
                    continue;

                yield return Path.IsPathRooted(assemblyLocation)
                    ? assemblyLocation
                    : Path.Combine(configurationRoot, assemblyLocation);
            }
        }
    }
}
