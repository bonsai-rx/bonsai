using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bonsai.Configuration
{
    public class ScriptExtensions : IDisposable
    {
        const string DefaultProjectFileName = "Extensions.csproj";

        readonly IDictionary<string, PackageReference> packageMap;
        readonly PackageConfiguration packageConfiguration;
        readonly TempDirectory assemblyFolder;

        public ScriptExtensions(PackageConfiguration configuration, string outputPath)
        {
            packageConfiguration = configuration;
            packageMap = configuration.GetPackageReferenceMap();
            assemblyFolder = new TempDirectory(outputPath);
            ProjectFileName = Path.GetFullPath(DefaultProjectFileName);
        }

        public string ProjectFileName { get; private set; }

        public AssemblyName AssemblyName { get; internal set; }

        public bool DebugScripts { get; set; }

        public ScriptExtensionsProjectMetadata LoadProjectMetadata()
        {
            if (!File.Exists(ProjectFileName))
                return default;

            using var stream = File.OpenRead(ProjectFileName);
            return new ScriptExtensionsProjectMetadata(stream);
        }

        public void UpdateProjectMetadata(ScriptExtensionsProjectMetadata projectMetadata)
            => File.WriteAllText(ProjectFileName, projectMetadata.GetProjectXml());

        public void AddAssemblyReferences(IEnumerable<string> assemblyReferences)
        {
            var projectMetadata = LoadProjectMetadata();
            var packageReferences = assemblyReferences
                .Select(assemblyName => packageConfiguration.GetAssemblyPackageReference(assemblyName, packageMap))
                .Where(package => package is not null);

            projectMetadata = projectMetadata.AddPackageReferences(packageReferences);
            UpdateProjectMetadata(projectMetadata);
        }

        public void Dispose()
        {
            assemblyFolder.Dispose();
        }
    }
}
