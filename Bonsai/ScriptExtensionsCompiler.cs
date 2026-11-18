using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Bonsai.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace Bonsai
{
    static class ScriptExtensionsCompiler
    {
        const string OutputAssemblyName = "Extensions";
        const string ProjectExtension = ".csproj";
        const string ResponseFileExtension = ".rsp";
        const string ScriptPattern = "*.cs";
        const string AssemblyExtension = ".dll";
        const string CompilerName = "Bonsai.Build";
        const string ExecutableExtension = ".exe";
        const string DotNetHost = "dotnet";
        const string PortableCompilerDirectory = "any";
        const string WindowsCompilerDirectory = "win-x64";
        const int CompilerErrorExitCode = 1;
        static readonly string CompilerDirectory = Path.Combine("tools", "net8.0");
        static readonly string[] ImplicitPackageReferences = new[] { "Bonsai.Core" };

        internal static ScriptExtensions CompileAssembly(
            NuGetFramework projectFramework,
            PackageConfiguration configuration,
            string editorRepositoryPath,
            PackageIdentity editorPackageName,
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
            var assemblyFile = Path.Combine(assemblyDirectory, OutputAssemblyName + AssemblyExtension);
            var responseFile = Path.Combine(assemblyDirectory, OutputAssemblyName + ResponseFileExtension);
            File.WriteAllLines(responseFile, GetCompilerArguments(
                scriptFiles,
                assemblyReferences,
                assemblyFile,
                projectMetadata,
                includeDebugInformation));

            var output = new List<string>();
            var compilerStartInfo = GetCompilerStartInfo(editorRepositoryPath, editorPackageName, responseFile);
            var exitCode = compilerStartInfo is null ? null : RunCompiler(compilerStartInfo, output);
            if (exitCode != 0)
            {
                try
                {
                    Console.Error.WriteLine("--- Error building script extensions ---");
                    if (compilerStartInfo is null)
                        Console.Error.WriteLine($"The script compiler '{CompilerName}' could not be found.");
                    else if (exitCode != CompilerErrorExitCode)
                        Console.Error.WriteLine($"The script compiler could not be started. Compiling extension scripts requires the .NET 8 runtime or later.");
                    foreach (var line in output)
                    {
                        Console.Error.WriteLine(line);
                    }
                }
                finally { scriptEnvironment.Dispose(); }
                return new ScriptExtensions(configuration, null);
            }

            scriptEnvironment.RegisterAssembly(assemblyFile);
            return scriptEnvironment;
        }

        static IEnumerable<string> GetCompilerArguments(
            IEnumerable<string> scriptFiles,
            IEnumerable<string> assemblyReferences,
            string assemblyFile,
            ScriptExtensionsProjectMetadata projectMetadata,
            bool includeDebugInformation)
        {
            yield return "/nostdlib+";
            yield return "/nologo";
            yield return "/target:library";
            yield return "/checksumalgorithm:sha256";
            yield return $"/out:{Quote(assemblyFile)}";
            if (includeDebugInformation)
            {
                yield return "/debug:portable";
                yield return "/optimize-";
            }
            else yield return "/optimize+";

            if (projectMetadata.AllowUnsafeBlocks)
                yield return "/unsafe+";
            if (!string.IsNullOrEmpty(projectMetadata.LangVersion))
                yield return $"/langversion:{projectMetadata.LangVersion}";

            foreach (var reference in assemblyReferences)
                yield return $"/reference:{Quote(reference)}";
            foreach (var scriptFile in scriptFiles)
                yield return Quote(scriptFile);
        }

        static string Quote(string path) => $"\"{path}\"";

        static ProcessStartInfo GetCompilerStartInfo(
            string editorRepositoryPath,
            PackageIdentity editorPackageName,
            string responseFile)
        {
            var arguments = Quote("@" + responseFile);
            var pathResolver = new PackagePathResolver(editorRepositoryPath);
            var editorDirectory = Path.GetDirectoryName(typeof(ScriptExtensionsCompiler).Assembly.Location);
            var useWindowsCompiler =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
                RuntimeInformation.OSArchitecture == Architecture.X64;
            foreach (var rootDirectory in new[] { pathResolver.GetInstalledPath(editorPackageName), editorDirectory })
            {
                if (rootDirectory is null)
                    continue;

                var toolsDirectory = Path.Combine(rootDirectory, CompilerDirectory);
                var windowsCompiler = Path.Combine(toolsDirectory, WindowsCompilerDirectory, CompilerName + ExecutableExtension);
                if (useWindowsCompiler && File.Exists(windowsCompiler))
                    return CreateStartInfo(windowsCompiler, arguments);

                var portableCompiler = Path.Combine(toolsDirectory, PortableCompilerDirectory, CompilerName + AssemblyExtension);
                if (File.Exists(portableCompiler))
                    return CreateStartInfo(DotNetHost, $"{Quote(portableCompiler)} {arguments}");
            }

            return null;

            static ProcessStartInfo CreateStartInfo(string fileName, string arguments) => new(fileName, arguments)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
        }

        static int? RunCompiler(ProcessStartInfo startInfo, List<string> output)
        {
            using var process = new Process { StartInfo = startInfo };
            process.OutputDataReceived += (sender, e) => AddOutput(e.Data);
            process.ErrorDataReceived += (sender, e) => AddOutput(e.Data);
            try { process.Start(); }
            catch (Win32Exception ex)
            {
                AddOutput(ex.Message);
                return null;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            return process.ExitCode;

            void AddOutput(string line)
            {
                if (line is null) return;
                lock (output) output.Add(line);
            }
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
