using Bonsai.Configuration;
using Bonsai.Design;
using Bonsai.NuGet;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml;

namespace Bonsai
{
    sealed class DependencyInspector : MarshalByRefObject
    {
        const string XsiAttributeValue = "http://www.w3.org/2001/XMLSchema-instance";
        const string WorkflowElementName = "Workflow";
        const string ExpressionElementName = "Expression";
        const string IncludeWorkflowTypeName = "IncludeWorkflow";
        const string PathAttributeName = "Path";
        const string TypeAttributeName = "type";
        const char AssemblySeparator = ':';

        static IEnumerable<VisualizerDialogSettings> GetVisualizerSettings(VisualizerLayout root)
        {
            var stack = new Stack<VisualizerLayout>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var layout = stack.Pop();
                foreach (var settings in layout.DialogSettings)
                {
                    yield return settings;
                    if (settings.NestedLayout != null)
                    {
                        stack.Push(settings.NestedLayout);
                    }
                }
            }
        }

        static Configuration.PackageReference[] GetPackageDependencies(string[] fileNames, PackageConfiguration configuration)
        {
            using var context = LoaderResource.CreateMetadataLoadContext(configuration);
            var scriptEnvironment = new ScriptExtensions(configuration, null);
            var assemblies = new HashSet<Assembly>();
            foreach (var path in fileNames)
            {
                var metadata = WorkflowBuilder.ReadMetadata(path);
                using (var markupReader = new StringReader(metadata.WorkflowMarkup))
                using (var reader = XmlReader.Create(markupReader))
                {
                    reader.ReadToFollowing(WorkflowElementName);
                    using (var workflowReader = reader.ReadSubtree())
                    {
                        while (workflowReader.ReadToFollowing(ExpressionElementName))
                        {
                            if (!workflowReader.HasAttributes) continue;
                            if (workflowReader.GetAttribute(TypeAttributeName, XsiAttributeValue) == IncludeWorkflowTypeName)
                            {
                                var includePath = workflowReader.GetAttribute(PathAttributeName);
                                var separatorIndex = includePath != null ? includePath.IndexOf(AssemblySeparator) : -1;
                                if (separatorIndex >= 0 && !Path.IsPathRooted(includePath))
                                {
                                    var assemblyName = includePath.Split(new[] { AssemblySeparator }, 2)[0];
                                    if (!string.IsNullOrEmpty(assemblyName))
                                    {
                                        var assembly = context.LoadFromAssemblyName(assemblyName);
                                        assemblies.Add(assembly);
                                    }
                                }
                            }
                        }
                    }
                }

                assemblies.Add(typeof(WorkflowBuilder).Assembly);
                assemblies.AddRange(metadata.GetExtensionTypes().Select(type => type.Assembly));

                var layoutPath = Path.ChangeExtension(path, Path.GetExtension(path) + Constants.LayoutExtension);
                if (File.Exists(layoutPath))
                {
                    var visualizerMap = new Lazy<IDictionary<string, Type>>(() =>
                        TypeVisualizerLoader.GetVisualizerTypes(configuration)
                                            .Select(descriptor => descriptor.VisualizerTypeName).Distinct()
                                            .Select(typeName => Type.GetType(typeName, false))
                                            .Where(type => type != null)
                                            .ToDictionary(type => type.FullName)
                                            .Wait());

                    using (var reader = XmlReader.Create(layoutPath))
                    {
                        var layout = (VisualizerLayout)VisualizerLayout.Serializer.Deserialize(reader);
                        foreach (var settings in GetVisualizerSettings(layout))
                        {
                            var typeName = settings.VisualizerTypeName;
                            if (typeName == null) continue;
                            if (visualizerMap.Value.TryGetValue(typeName, out Type type))
                            {
                                assemblies.Add(type.Assembly);
                            }
                        }
                    }
                }
            }

            var packageMap = configuration.GetPackageReferenceMap();
            var dependencies = assemblies.Select(assembly =>
                configuration.GetAssemblyPackageReference(assembly.GetName().Name, packageMap))
                .Where(package => package != null);

            var scriptProjectMetadata = scriptEnvironment.LoadProjectMetadata();
            if (scriptProjectMetadata.Exists)
            {
                dependencies = dependencies.Concat(
                    from id in scriptProjectMetadata.GetPackageReferences()
                    where configuration.Packages.Contains(id)
                    select configuration.Packages[id]);
            }

            return dependencies.ToArray();
        }

        public static IObservable<PackageDependency> GetWorkflowPackageDependencies(string[] fileNames, PackageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return from dependency in GetPackageDependencies(fileNames, configuration).ToObservable()
                   let versionRange = new VersionRange(NuGetVersion.Parse(dependency.Version), includeMinVersion: true)
                   select new PackageDependency(dependency.Id, versionRange);
        }
    }
}
