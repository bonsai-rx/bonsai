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
using System.Xml.Linq;

namespace Bonsai
{
    static class ScriptExtensionsProvider
    {
        const string OutputAssemblyName = "Extensions";
        const string PackageReferenceElement = "PackageReference";
        const string PackageIncludeAttribute = "Include";
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

        public static ScriptExtensionsEnvironment CompileAssembly(PackageConfiguration configuration, string editorRepositoryPath, bool includeDebugInformation)
        {
            var assemblyNames = new HashSet<string>();
            assemblyNames.Add("System.dll");
            assemblyNames.Add("System.Core.dll");
            assemblyNames.Add("System.Reactive.Linq.dll");
            assemblyNames.Add("Bonsai.Core.dll");

            var path = Environment.CurrentDirectory;
            var configurationRoot = ConfigurationHelper.GetConfigurationRoot(configuration);
            var scriptProjectFile = Path.Combine(path, Path.ChangeExtension(OutputAssemblyName, ProjectExtension));
            if (!File.Exists(scriptProjectFile)) return new ScriptExtensionsEnvironment(configuration, null);

            var document = XmlUtility.LoadSafe(scriptProjectFile);
            var packageRepository = new LocalPackageRepository(editorRepositoryPath);
            var projectReferences = from element in document.Descendants(XName.Get(PackageReferenceElement))
                                    let package = packageRepository.FindPackage(element.Attribute(XName.Get(PackageIncludeAttribute)).Value)
                                    where package != null
                                    from assemblyReference in FindAssemblyReferences(packageRepository, package)
                                    select assemblyReference;
            assemblyNames.AddRange(projectReferences);

            var scriptFiles = Directory.GetFiles(path, ScriptExtension, SearchOption.AllDirectories);
            if (scriptFiles.Length == 0) return new ScriptExtensionsEnvironment(configuration, null);

            var assemblyDirectory = Path.GetTempPath() + OutputAssemblyName + "." + Guid.NewGuid().ToString();
            var scriptEnvironment = new ScriptExtensionsEnvironment(configuration, assemblyDirectory);
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
                    return new ScriptExtensionsEnvironment(configuration, null);
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
