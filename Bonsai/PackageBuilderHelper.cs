using Bonsai.Configuration;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai
{
    static class PackageBuilderHelper
    {
        static string GetRelativePath(string path, string basePath)
        {
            var pathUri = new Uri(path);
            var rootUri = new Uri(basePath);
            var relativeUri = rootUri.MakeRelativeUri(pathUri);
            return relativeUri.ToString().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        static IEnumerable<ManifestFile> GetContentFiles(string basePath)
        {
            return from file in Directory.GetFiles(basePath, "*", SearchOption.AllDirectories)
                   let extension = Path.GetExtension(file)
                   where extension != global::NuGet.Constants.ManifestExtension &&
                         extension != global::NuGet.Constants.PackageExtension
                   select new ManifestFile
                   {
                       Source = file,
                       Target = Path.Combine("content", GetRelativePath(file, basePath))
                   };
        }

        public static Manifest CreatePackageManifest(string metadataPath)
        {
            if (File.Exists(metadataPath))
            {
                using (var stream = File.OpenRead(metadataPath))
                {
                    return Manifest.ReadFrom(stream, true);
                }
            }
            else
            {
                var manifest = new Manifest();
                manifest.Metadata = new ManifestMetadata()
                {
                    Authors = Environment.UserName,
                    Version = "1.0.0",
                    Id = Path.GetFileNameWithoutExtension(metadataPath),
                    Description = "My workflow description."
                };
                return manifest;
            }
        }

        public static PackageBuilder CreateExecutablePackage(string path, Manifest manifest, PackageConfiguration configuration, out bool updateDependencies)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                throw new ArgumentException("Invalid workflow file path.", "path");
            }

            var packageBuilder = new PackageBuilder();
            var basePath = Path.GetDirectoryName(path) + "\\";
            packageBuilder.Populate(manifest.Metadata);
            packageBuilder.Tags.Add(NuGet.Constants.BonsaiDirectory);
            packageBuilder.Tags.Add(NuGet.Constants.GalleryDirectory);
            var files = manifest.Files ?? GetContentFiles(basePath);
            packageBuilder.PopulateFiles(basePath, files);
            var manifestDependencies = new Dictionary<string, PackageDependency>(StringComparer.OrdinalIgnoreCase);
            foreach (var dependency in packageBuilder.DependencySets.Where(set => set.TargetFramework == null)
                                                                    .SelectMany(set => set.Dependencies))
            {
                manifestDependencies.Add(dependency.Id, dependency);
            }

            updateDependencies = false;
            var workflowFiles = packageBuilder.Files.Select(file => file as PhysicalPackageFile)
                                                    .Where(file => file != null && Path.GetExtension(file.SourcePath) == NuGet.Constants.BonsaiExtension)
                                                    .Select(file => file.SourcePath)
                                                    .ToArray();
            var workflowDependencies = DependencyInspector.GetWorkflowPackageDependencies(workflowFiles, configuration).ToArray().Wait();
            foreach (var dependency in workflowDependencies)
            {
                PackageDependency manifestDependency;
                if (!manifestDependencies.TryGetValue(dependency.Id, out manifestDependency) ||
                    !DependencyEqualityComparer.Default.Equals(dependency, manifestDependency))
                {
                    updateDependencies = true;
                    manifestDependencies[dependency.Id] = dependency;
                }
            }

            var dependencySet = new PackageDependencySet(null, manifestDependencies.Values);
            packageBuilder.DependencySets.Clear();
            packageBuilder.DependencySets.Add(dependencySet);
            return packageBuilder;
        }

        class DependencyEqualityComparer : IEqualityComparer<PackageDependency>
        {
            public static readonly DependencyEqualityComparer Default = new DependencyEqualityComparer();

            public bool Equals(PackageDependency x, PackageDependency y)
            {
                return x.Id.Equals(y.Id, StringComparison.OrdinalIgnoreCase) &&
                       x.VersionSpec.IsMaxInclusive == y.VersionSpec.IsMaxInclusive &&
                       x.VersionSpec.IsMinInclusive == y.VersionSpec.IsMinInclusive &&
                       x.VersionSpec.MaxVersion == y.VersionSpec.MaxVersion &&
                       x.VersionSpec.MinVersion == y.VersionSpec.MinVersion;
            }

            public int GetHashCode(PackageDependency obj)
            {
                return obj.Id.GetHashCode() ^
                       obj.VersionSpec.IsMaxInclusive.GetHashCode() ^
                       obj.VersionSpec.IsMinInclusive.GetHashCode() ^
                       EqualityComparer<SemanticVersion>.Default.GetHashCode(obj.VersionSpec.MaxVersion) ^
                       EqualityComparer<SemanticVersion>.Default.GetHashCode(obj.VersionSpec.MinVersion);
            }
        }
    }
}
