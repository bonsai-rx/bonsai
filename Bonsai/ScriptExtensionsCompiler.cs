using System;
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
            var packageReferences = ImplicitPackageReferences.Concat(projectMetadata.GetPackageReferences());
            var assemblyReferences = FrameworkReferences
                .GetReferencePaths(projectMetadata.GetAssemblyReferences())
                .Concat(ScriptExtensionsProvider.GetPackageAssemblyLocations(
                    projectFramework,
                    configuration,
                    editorRepositoryPath,
                    packageReferences))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            var compiler = new AssemblyCompiler
            {
                AssemblyName = OutputAssemblyName,
                IncludeDebugInformation = includeDebugInformation,
                AllowUnsafeBlocks = projectMetadata.AllowUnsafeBlocks,
                LanguageVersion = projectMetadata.LangVersion
            };

            var assemblyFile = Path.Combine(assemblyDirectory, OutputAssemblyName + AssemblyExtension);
            var result = compiler.Compile(scriptFiles.Select(SourceFile.FromFile), assemblyReferences, assemblyFile);
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
    }
}
