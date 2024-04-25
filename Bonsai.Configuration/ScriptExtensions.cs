using NuGet.Configuration;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using static Bonsai.Configuration.ScriptExtensionsProjectMetadata;

namespace Bonsai.Configuration
{
    public class ScriptExtensions : IDisposable
    {
        const string OuterIndent = "  ";
        const string InnerIndent = "    ";
        const string DefaultProjectFileName = "Extensions.csproj";
        const string ProjectFileTemplate = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>

</Project>";
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
                var machineWideSettings = new NuGet.BonsaiMachineWideSettings();
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

            var readerSettings = new XmlReaderSettings()
            {
                IgnoreWhitespace = true,
                IgnoreProcessingInstructions = true,
                DtdProcessing = DtdProcessing.Prohibit
            };

            using var stream = File.OpenRead(ProjectFileName);
            using var reader = XmlReader.Create(stream, readerSettings);
            var document = XDocument.Load(reader, LoadOptions.None);
            return new ScriptExtensionsProjectMetadata(document);
        }

        public void AddAssemblyReferences(IEnumerable<string> assemblyReferences)
        {
            XElement root;
            if (!File.Exists(ProjectFileName))
            {
                root = XElement.Parse(ProjectFileTemplate, LoadOptions.PreserveWhitespace);
                EnsureNuGetSettings();
            }
            else root = XElement.Load(ProjectFileName, LoadOptions.PreserveWhitespace);

            var projectReferences = root.Descendants(PackageReferenceElement).ToArray();
            var lastReference = projectReferences.LastOrDefault();

            var packageReferences = assemblyReferences
                .Select(assemblyName => packageConfiguration.GetAssemblyPackageReference(assemblyName, packageMap))
                .Where(package => package != null);
            foreach (var reference in packageReferences)
            {
                var includeAttribute = new XAttribute(PackageIncludeAttribute, reference.Id);
                var versionAttribute = new XAttribute(PackageVersionAttribute, reference.Version);
                var existingReference = (from element in projectReferences
                                         let id = element.Attribute(PackageIncludeAttribute)
                                         where id != null && id.Value == reference.Id
                                         select element).FirstOrDefault();
                if (existingReference != null)
                {
                    var version = existingReference.Attribute(PackageVersionAttribute);
                    if (version == null) existingReference.Add(versionAttribute);
                    else if (NuGetVersion.Parse(version.Value) < NuGetVersion.Parse(reference.Version))
                    {
                        version.SetValue(reference.Version);
                    }

                    continue;
                }

                var referenceElement = new XElement(PackageReferenceElement, includeAttribute, versionAttribute);
                if (lastReference == null)
                {
                    var itemGroup = new XElement(ItemGroupElement);
                    itemGroup.Add(Environment.NewLine + InnerIndent);
                    itemGroup.Add(referenceElement);
                    itemGroup.Add(Environment.NewLine + OuterIndent);

                    root.Add(OuterIndent);
                    root.Add(itemGroup);
                    root.Add(Environment.NewLine);
                    root.Add(Environment.NewLine);
                }
                else
                {
                    lastReference.AddAfterSelf(referenceElement);
                    if (lastReference.PreviousNode != null && lastReference.PreviousNode.NodeType >= XmlNodeType.Text)
                    {
                        lastReference.AddAfterSelf(lastReference.PreviousNode);
                    }
                }
                lastReference = referenceElement;
            }

            File.WriteAllText(ProjectFileName, root.ToString(SaveOptions.DisableFormatting));
        }

        public void Dispose()
        {
            assemblyFolder.Dispose();
        }
    }
}
