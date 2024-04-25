#if NET472_OR_GREATER
using Microsoft.CSharp;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
                        yield return Path.GetFileName(assembly);
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
                                yield return assemblyName + DllExtension;
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

            var assemblyNames = new HashSet<string>();
            var assemblyDirectory = Path.GetTempPath() + OutputAssemblyName + "." + Guid.NewGuid().ToString();
            var scriptEnvironment = new ScriptExtensions(configuration, assemblyDirectory);
            var scriptProjectMetadata = scriptEnvironment.LoadProjectMetadata();
            var packageSource = new PackageSource(editorRepositoryPath);
            var packageRepository = new SourceRepository(packageSource, Repository.Provider.GetCoreV3());
            var dependencyResource = packageRepository.GetResource<DependencyInfoResource>();
            var localPackageResource = packageRepository.GetResource<FindLocalPackagesResource>();
            using (var cacheContext = new SourceCacheContext())
            {
                var projectReferences = from id in scriptProjectMetadata.GetPackageReferences()
                                        from assemblyReference in FindAssemblyReferences(
                                            projectFramework,
                                            dependencyResource,
                                            localPackageResource,
                                            cacheContext,
                                            packageId: id)
                                        select assemblyReference;
                assemblyNames.AddRange(scriptProjectMetadata.GetAssemblyReferences());
                assemblyNames.AddRange(projectReferences);
            }

            var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
            var runtimeAssemblyMap = Directory
                .EnumerateFiles(runtimeDirectory, "*" + DllExtension)
                .ToDictionary(Path.GetFileNameWithoutExtension);
            var facadesDirectory = Path.Combine(runtimeDirectory, "Facades");
            if (Directory.Exists(facadesDirectory) &&
                Directory.GetFiles(facadesDirectory, "netstandard.dll").FirstOrDefault() is string netstandardLocation)
            {
                var netstandardName = Path.GetFileNameWithoutExtension(netstandardLocation);
                runtimeAssemblyMap[netstandardName] = netstandardLocation;
            }

            var assemblyFile = Path.Combine(assemblyDirectory, OutputAssemblyName + DllExtension);
            var assemblyReferences = assemblyNames.Select(GetAssemblyLocation).ToArray();
            string GetAssemblyLocation(string fileName)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(fileName);
                var assemblyLocation = ConfigurationHelper.GetAssemblyLocation(configuration, assemblyName);
                if (assemblyLocation != null || runtimeAssemblyMap.TryGetValue(assemblyName, out assemblyLocation))
                {
                    return Path.IsPathRooted(assemblyLocation)
                        ? assemblyLocation
                        : Path.Combine(configurationRoot, assemblyLocation);
                }

                return fileName;
            }


            var compilerOptions = new List<string>();
            {
                if (scriptProjectMetadata.AllowUnsafeBlocks)
                    compilerOptions.Add("/unsafe");

                if (!includeDebugInformation)
                    compilerOptions.Add("/optimize");
            }

            var compilerParameters = new CompilerParameters(assemblyReferences, assemblyFile)
            {
                GenerateExecutable = false,
                GenerateInMemory = false,
                IncludeDebugInformation = includeDebugInformation,
                CompilerOptions = string.Join(" ", compilerOptions),
            };

            using (var codeProvider = new CSharpCodeProvider())
            {
                var results = codeProvider.CompileAssemblyFromFile(compilerParameters, scriptFiles);
                if (results.Errors.HasErrors)
                {
                    try
                    {
                        Console.Error.WriteLine("--- Error building script extensions ---");
                        foreach (var error in results.Errors)
                        {
                            Console.Error.WriteLine(error);
                        }
                    }
                    finally { scriptEnvironment.Dispose(); }
                    return new ScriptExtensions(configuration, null);
                }
                else
                {
                    var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
                    configuration.AssemblyReferences.Add(assemblyName.Name);
                    configuration.AssemblyLocations.Add(assemblyName.Name, ProcessorArchitecture.MSIL, assemblyName.CodeBase);
                    scriptEnvironment.AssemblyName = assemblyName;
                }
                return scriptEnvironment;
            }
        }
    }
}
#endif
