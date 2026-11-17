using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bonsai.Build;
using Bonsai.Configuration;
using NuGet.Frameworks;

namespace Bonsai
{
    static class ScriptExtensionsCompiler
    {
        const string OutputAssemblyName = "Extensions";
        const string ProjectExtension = ".csproj";
        const string ScriptPattern = "*.cs";
        const string AssemblyExtension = ".dll";
        static readonly string[] ImplicitPackageReferences = new[] { "Bonsai.Core" };

        internal static ScriptExtensions CompileAssembly(
            NuGetFramework projectFramework,
            PackageConfiguration configuration,
            string editorRepositoryPath,
            bool includeDebugInformation)
        {
            var path = Environment.CurrentDirectory;
            var scriptProjectFile = Path.Combine(path, OutputAssemblyName + ProjectExtension);
            if (!File.Exists(scriptProjectFile)) return new ScriptExtensions(configuration, null);

            var extensionsPath = Path.Combine(path, OutputAssemblyName);
            if (!Directory.Exists(extensionsPath)) return new ScriptExtensions(configuration, null);

            var scriptFiles = Directory.GetFiles(extensionsPath, ScriptPattern, SearchOption.AllDirectories);
            if (scriptFiles.Length == 0) return new ScriptExtensions(configuration, null);

            var assemblyDirectory = Path.GetTempPath() + OutputAssemblyName + "." + Guid.NewGuid().ToString();
            var scriptEnvironment = new ScriptExtensions(configuration, assemblyDirectory);
            var projectMetadata = scriptEnvironment.LoadProjectMetadata();
            var assemblyReferences = GetAssemblyReferences(projectFramework, configuration, editorRepositoryPath, projectMetadata);

            var compiler = new AssemblyCompiler
            {
                AssemblyName = OutputAssemblyName,
                IncludeDebugInformation = includeDebugInformation,
                AllowUnsafeBlocks = projectMetadata.AllowUnsafeBlocks,
                LanguageVersion = projectMetadata.LangVersion
            };

            var assemblyFile = Path.Combine(assemblyDirectory, OutputAssemblyName + AssemblyExtension);
            var result = compiler.Compile(scriptFiles, assemblyReferences, assemblyFile);
            if (!result.Success)
            {
                try
                {
                    Console.Error.WriteLine("--- Error building script extensions ---");
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        Console.Error.WriteLine(diagnostic);
                    }
                }
                finally { scriptEnvironment.Dispose(); }
                return new ScriptExtensions(configuration, null);
            }

            scriptEnvironment.RegisterAssembly(assemblyFile);
            return scriptEnvironment;
        }

        static IEnumerable<string> GetAssemblyReferences(
            NuGetFramework projectFramework,
            PackageConfiguration configuration,
            string editorRepositoryPath,
            ScriptExtensionsProjectMetadata projectMetadata)
        {
            var packageReferences = ImplicitPackageReferences.Concat(projectMetadata.GetPackageReferences());
            var packageAssemblyNames = ScriptExtensionsProvider.GetPackageAssemblyNames(
                projectFramework,
                editorRepositoryPath,
                packageReferences);

            var configurationRoot = ConfigurationHelper.GetConfigurationRoot(configuration);
            var packageAssemblyLocations = new List<string>();
            var unresolvedAssemblyNames = new List<string>();
            foreach (var assemblyName in packageAssemblyNames)
            {
                var assemblyLocation = configuration.GetAssemblyLocation(assemblyName);
                if (assemblyLocation is null)
                    unresolvedAssemblyNames.Add(assemblyName);
                else
                    packageAssemblyLocations.Add(Path.IsPathRooted(assemblyLocation)
                        ? assemblyLocation
                        : Path.Combine(configurationRoot, assemblyLocation));
            }

            var frameworkReferences = FrameworkReferences
                .GetReferencePaths(projectMetadata.GetAssemblyReferences().Concat(unresolvedAssemblyNames))
                .ToArray();
            var frameworkAssemblyNames = new HashSet<string>(
                frameworkReferences.Select(Path.GetFileNameWithoutExtension),
                StringComparer.OrdinalIgnoreCase);
            var missingReferences = unresolvedAssemblyNames
                .Where(assemblyName => !frameworkAssemblyNames.Contains(assemblyName))
                .Select(assemblyName => assemblyName + AssemblyExtension);

            return frameworkReferences
                .Concat(packageAssemblyLocations)
                .Concat(missingReferences)
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}
