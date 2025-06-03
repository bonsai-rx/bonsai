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
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace Bonsai
{
    sealed class DependencyInspector : MarshalByRefObject
    {
        const string WorkflowElementName = "Workflow";
        const string ExpressionElementName = "Expression";
        const string IncludeWorkflowTypeName = "IncludeWorkflow";
        const string PathAttributeName = "Path";
        const string TypeAttributeName = "type";
        const char AssemblySeparator = ':';

        static async Task<IDictionary<string, Type>> GetVisualizerMap(PackageConfiguration configuration)
        {
            return await TypeVisualizerLoader
                .GetVisualizerTypes(configuration)
                .Select(descriptor => descriptor.VisualizerTypeName).Distinct()
                .Select(typeName => Type.GetType(typeName, false))
                .Where(type => type is not null)
                .ToDictionary(type => type.FullName);
        }

        static IEnumerable<VisualizerWindowSettings> GetVisualizerSettings(VisualizerLayout root)
        {
            var stack = new Stack<VisualizerLayout>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var layout = stack.Pop();
                foreach (var settings in layout.WindowSettings)
                {
                    yield return settings;
                    if (settings.NestedLayout != null)
                    {
                        stack.Push(settings.NestedLayout);
                    }
                }
            }
        }

        static IEnumerable<Assembly> GetWorkflowDependencies(string path, MetadataLoadContext context)
        {
            var metadata = WorkflowBuilder.ReadMetadata(path);
            using (var markupReader = new StringReader(metadata.WorkflowMarkup))
            using (var reader = XmlReader.Create(markupReader))
            {
                reader.ReadToFollowing(WorkflowElementName);
                using var workflowReader = reader.ReadSubtree();
                while (workflowReader.ReadToFollowing(ExpressionElementName))
                {
                    if (!workflowReader.HasAttributes) continue;
                    if (workflowReader.GetAttribute(TypeAttributeName, XmlSchema.InstanceNamespace) == IncludeWorkflowTypeName)
                    {
                        var includePath = workflowReader.GetAttribute(PathAttributeName);
                        var separatorIndex = includePath != null ? includePath.IndexOf(AssemblySeparator) : -1;
                        if (separatorIndex >= 0 && !Path.IsPathRooted(includePath))
                        {
                            var assemblyName = includePath.Split(new[] { AssemblySeparator }, 2)[0];
                            if (!string.IsNullOrEmpty(assemblyName))
                            {
                                var assembly = context.LoadFromAssemblyName(assemblyName);
                                yield return assembly;
                            }
                        }
                    }
                }
            }

            foreach (var assembly in metadata.GetExtensionTypes().Select(type => type.Assembly))
                yield return assembly;
        }

        static IEnumerable<Assembly> GetLayoutDependencies(string path, IDictionary<string, Type> visualizerMap)
        {
            var layout = VisualizerLayout.Load(path);
            foreach (var settings in GetVisualizerSettings(layout))
            {
                var typeName = settings.VisualizerTypeName;
                if (typeName == null) continue;
                if (visualizerMap.TryGetValue(typeName, out Type type))
                {
                    yield return type.Assembly;
                }
            }
        }

        public static Configuration.PackageReference[] GetPackageDependencies(IEnumerable<string> files, PackageConfiguration configuration)
        {
            using var context = LoaderResource.CreateMetadataLoadContext(configuration);
            var scriptEnvironment = new ScriptExtensions(configuration, null);
            var assemblies = new HashSet<Assembly> { typeof(WorkflowBuilder).Assembly };
            var visualizerMap = new Lazy<IDictionary<string, Type>>(() => GetVisualizerMap(configuration).Result);

            foreach (var path in files)
            {
                switch (Path.GetExtension(path))
                {
                    case Constants.BonsaiExtension:
                        assemblies.AddRange(GetWorkflowDependencies(path, context));
                        break;
                    case Constants.LayoutExtension:
                        assemblies.AddRange(GetLayoutDependencies(path, visualizerMap.Value));
                        break;
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

        static Configuration.PackageReference[] GetPackageDependencies(IEnumerable<IPackageFile> files, PackageConfiguration configuration)
        {
            return GetPackageDependencies(from file in files
                                          let packageFile = file as PhysicalPackageFile
                                          where packageFile is not null
                                          select packageFile.SourcePath,
                                          configuration);
        }

        public static IEnumerable<PackageDependency> GetWorkflowPackageDependencies(
            IEnumerable<IPackageFile> files,
            PackageConfiguration configuration)
        {
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            return from dependency in GetPackageDependencies(files, configuration)
                   let versionRange = new VersionRange(NuGetVersion.Parse(dependency.Version), includeMinVersion: true)
                   select new PackageDependency(dependency.Id, versionRange);
        }
    }
}
