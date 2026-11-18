using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        const string DotNetHost = "dotnet";
        const string StagingPrefix = "~";
        const int CompilerErrorExitCode = 1;
        const int PathHashLength = 16;
        const int CacheKeyLength = 64;
        static readonly TimeSpan StagingTimeout = TimeSpan.FromHours(1);
        static readonly string CompilerDirectory = Path.Combine("tools", "net8.0");
        static readonly string TempCacheDirectory = Path.Combine(Path.GetTempPath(), "Bonsai", OutputAssemblyName);
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

            var scriptEnvironment = new ScriptExtensions(configuration, null);
            var projectMetadata = scriptEnvironment.LoadProjectMetadata();
            var assemblyReferences = GetAssemblyReferences(projectFramework, configuration, editorRepositoryPath, projectMetadata).ToArray();
            var arguments = GetCompilerArguments(scriptFiles, assemblyReferences, projectMetadata, includeDebugInformation).ToArray();

            var cacheDirectory = GetCacheDirectory(path);
            var cacheEntryDirectory = Path.Combine(
                cacheDirectory,
                GetCacheKey(editorPackageName, arguments, scriptFiles, assemblyReferences));
            var assemblyFile = Path.Combine(cacheEntryDirectory, OutputAssemblyName + AssemblyExtension);
            if (!File.Exists(assemblyFile) &&
                !TryCompileAssembly(editorRepositoryPath, editorPackageName, arguments, cacheDirectory, cacheEntryDirectory))
            {
                return new ScriptExtensions(configuration, null);
            }

            DeleteStaleCacheEntries(cacheDirectory, cacheEntryDirectory);
            scriptEnvironment.RegisterAssembly(assemblyFile);
            return scriptEnvironment;
        }

        static bool TryCompileAssembly(
            string editorRepositoryPath,
            PackageIdentity editorPackageName,
            IEnumerable<string> arguments,
            string cacheDirectory,
            string cacheEntryDirectory)
        {
            var stagingDirectory = Path.Combine(cacheDirectory, StagingPrefix + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stagingDirectory);
            try
            {
                var stagingAssemblyFile = Path.Combine(stagingDirectory, OutputAssemblyName + AssemblyExtension);
                var responseFile = Path.Combine(stagingDirectory, OutputAssemblyName + ResponseFileExtension);
                File.WriteAllLines(responseFile, arguments.Append($"/out:{Quote(stagingAssemblyFile)}"));

                var output = new List<string>();
                var compilerStartInfo = GetCompilerStartInfo(editorRepositoryPath, editorPackageName, responseFile);
                var exitCode = compilerStartInfo is null ? null : RunCompiler(compilerStartInfo, output);
                if (exitCode != 0)
                {
                    Console.Error.WriteLine("--- Error building script extensions ---");
                    if (compilerStartInfo is null)
                        Console.Error.WriteLine($"The script compiler '{CompilerName}' could not be found.");
                    else if (exitCode != CompilerErrorExitCode)
                        Console.Error.WriteLine("The script compiler could not be started. Compiling extension scripts requires the .NET 8 runtime or later.");
                    foreach (var line in output)
                    {
                        Console.Error.WriteLine(line);
                    }
                    return false;
                }

                try { Directory.Move(stagingDirectory, cacheEntryDirectory); }
                catch (IOException) when (File.Exists(Path.Combine(cacheEntryDirectory, OutputAssemblyName + AssemblyExtension))) { }
                return true;
            }
            finally { DeleteDirectory(stagingDirectory); }
        }

        static string GetCacheDirectory(string path)
        {
            var cacheDirectory = Path.Combine(Editor.Project.GetBuildDirectory(path), OutputAssemblyName);
            if (IsWritableDirectory(cacheDirectory))
                return cacheDirectory;

            return Path.Combine(
                TempCacheDirectory,
                GetHash(Encoding.UTF8.GetBytes(Path.GetFullPath(path).ToUpperInvariant())).Substring(0, PathHashLength));
        }

        static bool IsWritableDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                using (File.Create(Path.Combine(path, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose)) { }
                return true;
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }

        static void DeleteStaleCacheEntries(string cacheDirectory, string cacheEntryDirectory)
        {
            foreach (var directory in Directory.EnumerateDirectories(cacheDirectory))
            {
                if (string.Equals(directory, cacheEntryDirectory, StringComparison.OrdinalIgnoreCase))
                    continue;

                var name = Path.GetFileName(directory);
                if (name.StartsWith(StagingPrefix))
                {
                    if (DateTime.UtcNow - Directory.GetCreationTimeUtc(directory) < StagingTimeout)
                        continue;
                }
                else if (name.Length != CacheKeyLength || !name.All(Uri.IsHexDigit))
                    continue;

                DeleteDirectory(directory);
            }
        }

        static void DeleteDirectory(string path)
        {
            try { Directory.Delete(path, recursive: true); }
            catch (DirectoryNotFoundException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        static string GetCacheKey(
            PackageIdentity editorPackageName,
            IEnumerable<string> arguments,
            IEnumerable<string> scriptFiles,
            IEnumerable<string> assemblyReferences)
        {
            var key = new StringBuilder();
            key.AppendLine(editorPackageName.ToString());
            foreach (var argument in arguments)
                key.AppendLine(argument);
            foreach (var scriptFile in scriptFiles)
                key.AppendLine(GetHash(File.ReadAllBytes(scriptFile)));
            foreach (var reference in assemblyReferences)
            {
                var file = new FileInfo(reference);
                key.AppendLine(file.Exists ? $"{file.Length} {file.LastWriteTimeUtc.Ticks}" : string.Empty);
            }

            return GetHash(Encoding.UTF8.GetBytes(key.ToString()));
        }

        static string GetHash(byte[] data)
        {
            using var hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(data)).Replace("-", string.Empty).ToLowerInvariant();
        }

        static IEnumerable<string> GetCompilerArguments(
            IEnumerable<string> scriptFiles,
            IEnumerable<string> assemblyReferences,
            ScriptExtensionsProjectMetadata projectMetadata,
            bool includeDebugInformation)
        {
            yield return "/nostdlib+";
            yield return "/nologo";
            yield return "/target:library";
            yield return "/checksumalgorithm:sha256";
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
            var pathResolver = new PackagePathResolver(editorRepositoryPath);
            var editorDirectory = Path.GetDirectoryName(typeof(ScriptExtensionsCompiler).Assembly.Location);
            foreach (var rootDirectory in new[] { pathResolver.GetInstalledPath(editorPackageName), editorDirectory })
            {
                if (rootDirectory is null)
                    continue;

                var compilerPath = Path.Combine(rootDirectory, CompilerDirectory, CompilerName + AssemblyExtension);
                if (File.Exists(compilerPath))
                {
                    return new ProcessStartInfo(DotNetHost, $"{Quote(compilerPath)} {Quote("@" + responseFile)}")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                }
            }

            return null;
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
