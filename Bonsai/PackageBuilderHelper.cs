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

        public static PackageBuilder CreateWorkflowPackage(string path, PackageConfiguration configuration)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                throw new ArgumentException("Invalid workflow file path.", "path");
            }

            var packageBuilder = new PackageBuilder();
            var basePath = Path.GetDirectoryName(path) + "\\";

            var files = new HashSet<ManifestFile>();
            var metadataPath = Path.ChangeExtension(path, global::NuGet.Constants.ManifestExtension);
            if (File.Exists(metadataPath))
            {
                using (var stream = File.OpenRead(metadataPath))
                {
                    var manifest = Manifest.ReadFrom(stream, true);
                    packageBuilder.Populate(manifest.Metadata);
                    if (manifest.Files != null)
                    {
                        files.AddRange(manifest.Files);
                    }
                }
            }
            else
            {
                packageBuilder.Populate(new ManifestMetadata()
                {
                    Authors = Environment.UserName,
                    Version = "1.0.0",
                    Id = Path.GetFileNameWithoutExtension(path),
                    Description = "My workflow description."
                });
            }

            packageBuilder.Tags.Add(NuGet.Constants.BonsaiDirectory);
            packageBuilder.Tags.Add(NuGet.Constants.GalleryDirectory);
            files.AddRange(from file in Directory.GetFiles(basePath, "*", SearchOption.AllDirectories)
                           let extension = Path.GetExtension(file)
                           where extension != global::NuGet.Constants.ManifestExtension &&
                                 extension != global::NuGet.Constants.PackageExtension
                           select new ManifestFile
                           {
                               Source = file,
                               Target = Path.Combine("content", GetRelativePath(file, basePath))
                           });
            packageBuilder.PopulateFiles(basePath, files);

            packageBuilder.DependencySets.Clear();
            var dependencies = DependencyInspector.GetWorkflowPackageDependencies(path, configuration).ToArray().Wait();
            var dependencySet = new PackageDependencySet(null, dependencies);
            packageBuilder.DependencySets.Add(dependencySet);

            return packageBuilder;
        }
    }
}
