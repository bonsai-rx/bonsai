using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NuGet.Configuration;

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

        private void EnsureNuGetSettings()
        {
            var projectFolder = Path.GetDirectoryName(ProjectFileName);
            var settingsFileName = Path.Combine(projectFolder, Settings.DefaultSettingsFileName);
            if (!File.Exists(settingsFileName))
            {
                var machineWideSettings = new XPlatMachineWideSetting();
                var settings = machineWideSettings.Settings;
                if (settings != null)
                {
                    const string SectionPackageSources = "packageSources";
                    var packageSourceSection = settings.GetSection(SectionPackageSources);
                    if (packageSourceSection != null)
                    {
                        var packageSources = (from item in packageSourceSection.Items
                                              let source = item as SourceItem
                                              where source != null && Uri.IsWellFormedUriString(source.Value, UriKind.Absolute)
                                              select source).ToList();
                        if (packageSources.Count > 0)
                        {
                            var localSettings = new Settings(projectFolder);
                            foreach (var item in localSettings.GetSection(SectionPackageSources).Items)
                            {
                                localSettings.Remove(SectionPackageSources, item);
                            }

                            foreach (var source in packageSources)
                            {
                                localSettings.AddOrUpdate(SectionPackageSources, source);
                            }
                            localSettings.SaveToDisk();
                        }
                    }
                }
            }
        }

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
            if (!projectMetadata.Exists)
                EnsureNuGetSettings();

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
