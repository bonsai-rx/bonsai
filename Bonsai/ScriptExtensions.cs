using Bonsai.Configuration;
using Bonsai.Editor;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Bonsai
{
    class ScriptExtensions : IDisposable, IServiceProvider
    {
        const string OuterIndent = "  ";
        const string InnerIndent = "    ";
        const string DefaultProjectFileName = "Extensions.csproj";
        const string PackageReferenceElement = "PackageReference";
        const string PackageIncludeAttribute = "Include";
        const string PackageVersionAttribute = "Version";
        const string ItemGroupElement = "ItemGroup";
        const string ProjectFileTemplate = @"<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

</Project>";

        IDictionary<string, Configuration.PackageReference> packageMap;
        PackageConfiguration packageConfiguration;
        TempDirectory assemblyFolder;

        public ScriptExtensions(PackageConfiguration configuration, string outputPath)
        {
            packageConfiguration = configuration;
            packageMap = DependencyInspector.GetPackageReferenceMap(configuration);
            assemblyFolder = new TempDirectory(outputPath);
            ProjectFileName = Path.GetFullPath(DefaultProjectFileName);
        }

        public string ProjectFileName { get; private set; }

        public AssemblyName AssemblyName { get; internal set; }

        public bool DebugScripts { get; set; }

        private void EnsureNuGetSettings()
        {
            var projectFolder = Path.GetDirectoryName(ProjectFileName);
            var settingsFileName = Path.Combine(projectFolder, Constants.SettingsFileName);
            if (!File.Exists(settingsFileName))
            {
                var machineWideSettings = new Bonsai.NuGet.BonsaiMachineWideSettings();
                var settings = machineWideSettings.Settings.FirstOrDefault();
                if (settings != null)
                {
                    const string SectionPackageSources = "packageSources";
                    var packageSources = settings.GetValues(SectionPackageSources, false).ToList();
                    packageSources.RemoveAll(source => !Uri.IsWellFormedUriString(source.Value, UriKind.Absolute));
                    if (packageSources.Count > 0)
                    {
                        var fileSystem = new PhysicalFileSystem(projectFolder);
                        var localSettings = new Settings(fileSystem);
                        localSettings.SetValues(SectionPackageSources, packageSources);
                    }
                }
            }
        }

        public IEnumerable<string> GetPackageReferences()
        {
            if (!File.Exists(ProjectFileName)) return Enumerable.Empty<string>();
            var document = XmlUtility.LoadSafe(ProjectFileName);
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

            var packageReferences = DependencyInspector.GetAssemblyPackageReferences(packageConfiguration, assemblyReferences, packageMap);
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
                    else if (SemanticVersion.Parse(version.Value) < SemanticVersion.Parse(reference.Version))
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

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IScriptEnvironment))
            {
                return new ScriptExtensionsEnvironment(this);
            }

            return null;
        }

        public void Dispose()
        {
            assemblyFolder.Dispose();
        }

        class ScriptExtensionsEnvironment : IScriptEnvironment
        {
            readonly ScriptExtensions extensions;

            internal ScriptExtensionsEnvironment(ScriptExtensions owner)
            {
                extensions = owner;
            }

            public string ProjectFileName
            {
                get { return extensions.ProjectFileName; }
            }

            public AssemblyName AssemblyName
            {
                get { return extensions.AssemblyName; }
            }

            public bool DebugScripts
            {
                get { return extensions.DebugScripts; }
                set { extensions.DebugScripts = value; }
            }

            public void AddAssemblyReferences(IEnumerable<string> assemblyReferences)
            {
                extensions.AddAssemblyReferences(assemblyReferences);
            }
        }
    }
}
