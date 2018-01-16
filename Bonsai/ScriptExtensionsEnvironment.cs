﻿using Bonsai.Configuration;
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
    class ScriptExtensionsEnvironment : IScriptEnvironment, IDisposable, IServiceProvider
    {
        const string OuterIndent = "  ";
        const string InnerIndent = "    ";
        const string ProjectScriptFile = "Extensions.csproj";
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

        public ScriptExtensionsEnvironment(PackageConfiguration configuration, string outputPath)
        {
            packageConfiguration = configuration;
            packageMap = DependencyInspector.GetPackageReferenceMap(configuration);
            assemblyFolder = new TempDirectory(outputPath);
        }

        public AssemblyName AssemblyName { get; internal set; }

        public bool DebugScripts { get; set; }

        public void AddAssemblyReferences(IEnumerable<string> assemblyReferences)
        {
            XElement root;
            if (!File.Exists(ProjectScriptFile))
            {
                root = XElement.Parse(ProjectFileTemplate, LoadOptions.PreserveWhitespace);
            }
            else root = XElement.Load(ProjectScriptFile, LoadOptions.PreserveWhitespace);

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

            File.WriteAllText(ProjectScriptFile, root.ToString(SaveOptions.DisableFormatting));
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IScriptEnvironment))
            {
                return this;
            }

            return null;
        }

        public void Dispose()
        {
            assemblyFolder.Dispose();
        }
    }
}
