using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Bonsai.Configuration
{
    public class ScriptExtensions : IDisposable
    {
        const string OuterIndent = "  ";
        const string InnerIndent = "    ";
        const string DefaultProjectFileName = "Extensions.csproj";
        const string PackageReferenceElement = "PackageReference";
        const string PackageIncludeAttribute = "Include";
        const string PackageVersionAttribute = "Version";
        const string UseWindowsFormsElement = "UseWindowsForms";
        const string ItemGroupElement = "ItemGroup";
        const string ProjectFileTemplate = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>net472</TargetFramework>
  </PropertyGroup>

</Project>";

        IDictionary<string, Configuration.PackageReference> packageMap;
        PackageConfiguration packageConfiguration;
        TempDirectory assemblyFolder;

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

        static XDocument LoadProjectDocument(Stream stream)
        {
            using var reader = XmlReader.Create(stream, GetXmlReaderSettings());
            return XDocument.Load(reader, LoadOptions.None);
        }

        static XmlReaderSettings GetXmlReaderSettings()
        {
            return new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreProcessingInstructions = true,
                DtdProcessing = DtdProcessing.Prohibit
            };
        }

        public IEnumerable<string> GetAssemblyReferences()
        {
            yield return "System.dll";
            yield return "System.Core.dll";
            yield return "System.Drawing.dll";
            yield return "System.Reactive.Linq.dll";
            yield return "System.Xml.dll";
            yield return "Bonsai.Core.dll";

            if (!File.Exists(ProjectFileName)) yield break;
            using var stream = File.OpenRead(ProjectFileName);
            var document = LoadProjectDocument(stream);
            var useWindowsForms = document.Descendants(XName.Get(UseWindowsFormsElement)).FirstOrDefault();
            if (useWindowsForms != null && useWindowsForms.Value == "true")
            {
                yield return "System.Windows.Forms.dll";
            }
        }

        public IEnumerable<string> GetPackageReferences()
        {
            if (!File.Exists(ProjectFileName)) return Enumerable.Empty<string>();
            using var stream = File.OpenRead(ProjectFileName);
            var document = LoadProjectDocument(stream);
            return from element in document.Descendants(XName.Get(PackageReferenceElement))
                   let id = element.Attribute(PackageIncludeAttribute)
                   where id != null
                   select id.Value;
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

            var packageReferences = packageConfiguration.GetAssemblyPackageReferences(assemblyReferences, packageMap);
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
