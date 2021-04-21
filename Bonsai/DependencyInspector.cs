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
using System.Runtime.Loader;
using System.Xml;

namespace Bonsai
{
    sealed class DependencyInspector : IDisposable
    {
        readonly AssemblyLoadContext reflectionContext;
        readonly ScriptExtensions scriptEnvironment;
        readonly PackageConfiguration packageConfiguration;
        const string XsiAttributeValue = "http://www.w3.org/2001/XMLSchema-instance";
        const string WorkflowElementName = "Workflow";
        const string ExpressionElementName = "Expression";
        const string IncludeWorkflowTypeName = "IncludeWorkflow";
        const string PathAttributeName = "Path";
        const string TypeAttributeName = "type";
        const char AssemblySeparator = ':';

        public DependencyInspector(PackageConfiguration configuration)
        {
            reflectionContext = new AssemblyLoadContext("ReflectionOnly", isCollectible: true);
            ConfigurationHelper.SetAssemblyResolve(configuration, reflectionContext);
            scriptEnvironment = new ScriptExtensions(configuration, null);
            packageConfiguration = configuration;
        }

        IEnumerable<VisualizerDialogSettings> GetVisualizerSettings(VisualizerLayout root)
        {
            var stack = new Stack<VisualizerLayout>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var layout = stack.Pop();
                foreach (var settings in layout.DialogSettings)
                {
                    yield return settings;
                    var editorSettings = settings as WorkflowEditorSettings;
                    if (editorSettings != null && editorSettings.EditorVisualizerLayout != null)
                    {
                        stack.Push(editorSettings.EditorVisualizerLayout);
                    }
                }
            }
        }

        Configuration.PackageReference[] GetWorkflowPackageDependencies(string[] fileNames)
        {
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
                                        var assembly = Assembly.Load(assemblyName);
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
                        TypeVisualizerLoader.GetVisualizerTypes(packageConfiguration)
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

            var packageMap = packageConfiguration.GetPackageReferenceMap();
            var dependencies = packageConfiguration.GetAssemblyPackageReferences(
                assemblies.Select(assembly => assembly.GetName().Name),
                packageMap);
            if (File.Exists(scriptEnvironment.ProjectFileName))
            {
                dependencies = dependencies.Concat(
                    from id in scriptEnvironment.GetPackageReferences()
                    where packageConfiguration.Packages.Contains(id)
                    select packageConfiguration.Packages[id]);
            }

            return dependencies.ToArray();
        }

        public static IObservable<PackageDependency> GetWorkflowPackageDependencies(string[] fileNames, PackageConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            return Observable.Using(
                () => new DependencyInspector(configuration),
                resource => from dependency in resource.GetWorkflowPackageDependencies(fileNames).ToObservable()
                            let versionRange = new VersionRange(NuGetVersion.Parse(dependency.Version), includeMinVersion: true)
                            select new PackageDependency(dependency.Id, versionRange));
        }

        public void Dispose()
        {
            reflectionContext.Unload();
        }
    }
}
