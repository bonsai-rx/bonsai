using Bonsai.Configuration;
using Microsoft.CSharp;
using NuGet;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bonsai
{
    static class ScriptExtensionsProvider
    {
        const string OutputAssemblyName = "Extensions";
        const string ProjectExtension = ".csproj";
        const string ScriptExtension = "*.cs";
        const string DllExtension = ".dll";

        static IEnumerable<string> FindAssemblyReferences(IPackageRepository repository, IPackage package)
        {
            foreach (var assembly in package.AssemblyReferences)
            {
                yield return assembly.Name;
            }

            var dependencies = package.GetCompatiblePackageDependencies(null);
            foreach (var dependency in dependencies)
            {
                var dependencyPackage = repository.ResolveDependency(dependency, true, true);
                if (dependencyPackage != null)
                {
                    foreach (var reference in FindAssemblyReferences(repository, dependencyPackage))
                    {
                        yield return reference;
                    }
                }
            }
        }

        public static ScriptExtensions CompileAssembly(PackageConfiguration configuration, string editorRepositoryPath, bool includeDebugInformation)
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
            var packageRepository = new LocalPackageRepository(editorRepositoryPath);
            var projectReferences = from id in scriptEnvironment.GetPackageReferences()
                                    let package = packageRepository.FindPackage(id)
                                    where package != null
                                    from assemblyReference in FindAssemblyReferences(packageRepository, package)
                                    select assemblyReference;
            assemblyNames.Add("System.dll");
            assemblyNames.Add("System.Core.dll");
            assemblyNames.Add("System.Drawing.dll");
            assemblyNames.Add("System.Reactive.Linq.dll");
            assemblyNames.Add("Bonsai.Core.dll");
            assemblyNames.AddRange(projectReferences);

            var assemblyFile = Path.Combine(assemblyDirectory, Path.ChangeExtension(OutputAssemblyName, DllExtension));
            var assemblyReferences = (from fileName in assemblyNames
                                      let assemblyName = Path.GetFileNameWithoutExtension(fileName)
                                      let assemblyLocation = ConfigurationHelper.GetAssemblyLocation(configuration, assemblyName)
                                      select assemblyLocation == null ? fileName :
                                      Path.IsPathRooted(assemblyLocation) ? assemblyLocation :
                                      Path.Combine(configurationRoot, assemblyLocation))
                                      .ToArray();
            var compilerParameters = new CompilerParameters(assemblyReferences, assemblyFile);
            compilerParameters.GenerateExecutable = false;
            compilerParameters.GenerateInMemory = false;
            compilerParameters.IncludeDebugInformation = includeDebugInformation;
            if (!includeDebugInformation)
            {
                compilerParameters.CompilerOptions = "/optimize";
            }

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
