using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;

namespace Bonsai.Configuration
{
    public static class ScriptExtensionsProvider
    {
        const string OutputAssemblyName = "Extensions";
        const string ReferenceElement = "Reference";
        const string IncludeAttribute = "Include";
        const string PropertyExtension = ".props";
        const string ProjectExtension = ".csproj";
        const string ScriptExtension = "*.cs";
        const string DllExtension = ".dll";

        static IEnumerable<string> FindAssemblyReferences(
            NuGetFramework projectFramework,
            DependencyInfoResource dependencyResource,
            FindLocalPackagesResource localPackageResource,
            SourceCacheContext cacheContext,
            string packageId)
        {
            var packageInfo = localPackageResource.FindPackagesById(packageId, NullLogger.Instance, CancellationToken.None).FirstOrDefault();
            if (packageInfo == null) yield break;
            using (var reader = packageInfo.GetReader())
            {
                var assemblyReferences = reader.GetReferenceItems().GetNearest(projectFramework);
                if (assemblyReferences != null)
                {
                    foreach (var assembly in assemblyReferences.Items)
                    {
                        yield return Path.GetFileNameWithoutExtension(assembly);
                    }
                }

                var buildReferences = reader.GetBuildItems().GetNearest(projectFramework);
                if (buildReferences != null)
                {
                    var buildProperties = buildReferences.Items.FirstOrDefault(item => Path.GetExtension(item) == PropertyExtension);
                    if (buildProperties != null)
                    {
                        using (var propertyStream = reader.GetStream(buildProperties))
                        using (var propertyReader = XmlReader.Create(propertyStream))
                        {
                            while (propertyReader.ReadToFollowing(ReferenceElement))
                            {
                                var assemblyName = propertyReader.GetAttribute(IncludeAttribute);
                                yield return assemblyName;
                            }
                        }
                    }
                }
            }

            var dependencyInfo = dependencyResource.ResolvePackage(packageInfo.Identity, projectFramework, cacheContext, NullLogger.Instance, CancellationToken.None).Result;
            foreach (var dependency in dependencyInfo.Dependencies)
            {
                foreach (var reference in FindAssemblyReferences(projectFramework, dependencyResource, localPackageResource, cacheContext, dependency.Id))
                {
                    yield return reference;
                }
            }
        }

        public static ScriptExtensions CompileAssembly(
            NuGetFramework projectFramework,
            PackageConfiguration configuration,
            string editorRepositoryPath,
            bool includeDebugInformation)
        {
            var path = Environment.CurrentDirectory;
            var configurationRoot = ConfigurationHelper.GetConfigurationRoot(configuration);
            var scriptProjectFile = Path.Combine(path, Path.ChangeExtension(OutputAssemblyName, ProjectExtension));
            if (!File.Exists(scriptProjectFile)) return new ScriptExtensions(configuration, null);

            var extensionsPath = Path.Combine(path, OutputAssemblyName);
            if (!Directory.Exists(extensionsPath)) return new ScriptExtensions(configuration, null);

            var scriptFiles = Directory.GetFiles(extensionsPath, ScriptExtension, SearchOption.AllDirectories);
            if (scriptFiles.Length == 0) return new ScriptExtensions(configuration, null);

            HashSet<string> packageReferences;
            var assemblyDirectory = Path.GetTempPath() + OutputAssemblyName + "." + Guid.NewGuid().ToString();
            var scriptEnvironment = new ScriptExtensions(configuration, assemblyDirectory);
            var packageSource = new PackageSource(editorRepositoryPath);
            var packageRepository = new SourceRepository(packageSource, Repository.Provider.GetCoreV3());
            var dependencyResource = packageRepository.GetResource<DependencyInfoResource>();
            var localPackageResource = packageRepository.GetResource<FindLocalPackagesResource>();
            using (var cacheContext = new SourceCacheContext())
            {
                packageReferences = (from id in scriptEnvironment.GetPackageReferences()
                                     from assemblyReference in FindAssemblyReferences(
                                         projectFramework,
                                         dependencyResource,
                                         localPackageResource,
                                         cacheContext,
                                         packageId: id)
                                     select assemblyReference).ToHashSet();
            }

            var assemblyFile = Path.Combine(assemblyDirectory, Path.ChangeExtension(OutputAssemblyName, DllExtension));
            var assemblyReferences = (from assemblyName in packageReferences
                                      let assemblyLocation = ConfigurationHelper.GetAssemblyLocation(configuration, assemblyName)
                                      where assemblyLocation != null
                                      select (MetadataReference)MetadataReference.CreateFromFile(
                                          Path.IsPathRooted(assemblyLocation) ? assemblyLocation :
                                          Path.Combine(configurationRoot, assemblyLocation)))
                                          .ToList();
            assemblyReferences.AddRange(NetStandard20.All);
            var syntaxTrees = Array.ConvertAll(scriptFiles, scriptFile =>
            {
                var sourceText = File.ReadAllText(scriptFile);
                return CSharpSyntaxTree.ParseText(sourceText);
            });

            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: includeDebugInformation ? OptimizationLevel.Debug : OptimizationLevel.Release);
            var compilation = CSharpCompilation.Create(OutputAssemblyName, syntaxTrees, assemblyReferences, compilationOptions);
            var result = compilation.Emit(assemblyFile);
            if (result.Success)
            {
                var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
                configuration.AssemblyReferences.Add(assemblyName.Name);
                configuration.AssemblyLocations.Add(assemblyName.Name, ProcessorArchitecture.MSIL, assemblyFile);
                scriptEnvironment.AssemblyName = assemblyName;
                return scriptEnvironment;
            }
            else
            {
                try
                {
                    Console.Error.WriteLine("--- Error building script extensions ---");
                    foreach (var error in result.Diagnostics)
                    {
                        Console.Error.WriteLine(error);
                    }
                }
                finally { scriptEnvironment.Dispose(); }
                return new ScriptExtensions(configuration, null);
            }
        }
    }
}
