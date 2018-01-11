using Bonsai.Configuration;
using Microsoft.CSharp;
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
        const string ScriptExtension = "*.csx";
        const string DllExtension = ".dll";

        public static TempDirectory CompileAssembly(PackageConfiguration configuration, string output, bool includeDebugInformation)
        {
            var configurationRoot = ConfigurationHelper.GetConfigurationRoot(configuration);
            var scriptFiles = (from libraryFolder in configuration.LibraryFolders
                               let path = Path.Combine(configurationRoot, libraryFolder.Path)
                               where Directory.Exists(path)
                               from file in Directory.EnumerateFiles(path, ScriptExtension)
                               select file)
                               .ToArray();
            if (scriptFiles.Length == 0) return new TempDirectory(null);

            var references = new HashSet<string>();
            references.Add("System");
            references.Add("System.Core");
            var sources = new string[scriptFiles.Length];
            var regex = new Regex(@"#r ""(.*).dll""" + Environment.NewLine);
            for (int i = 0; i < scriptFiles.Length; i++)
            {
                sources[i] = File.ReadAllText(scriptFiles[i]);
                var matches = regex.Matches(sources[i]);
                for (int k = 0; k < matches.Count; k++)
                {
                    var match = matches[k];
                    if (!match.Success || match.Groups.Count < 2) continue;
                    references.Add(match.Groups[1].Value);
                }

                var lastMatch = matches.Count > 0 ? matches[matches.Count - 1] : null;
                if (lastMatch != null)
                {
                    var lineDirective = "#line " + (matches.Count + 1) + " \"" + scriptFiles[i] + "\"" + Environment.NewLine;
                    sources[i] = lineDirective + sources[i].Substring(lastMatch.Index + lastMatch.Length);
                }
            }

            var assemblyFolder = new TempDirectory(Path.GetTempPath() + output + "." + Guid.NewGuid().ToString());
            var assemblyFile = Path.Combine(assemblyFolder.Path, Path.ChangeExtension(output, DllExtension));
            var assemblyReferences = (from assemblyName in references
                                      let assemblyLocation = ConfigurationHelper.GetAssemblyLocation(configuration, assemblyName)
                                      select assemblyLocation == null ? assemblyName + DllExtension :
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
                var results = codeProvider.CompileAssemblyFromSource(compilerParameters, sources);
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
                    finally { assemblyFolder.Dispose(); }
                    return new TempDirectory(null);
                }
                else
                {
                    var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
                    configuration.AssemblyReferences.Add(assemblyName.Name);
                    configuration.AssemblyLocations.Add(assemblyName.Name, ProcessorArchitecture.MSIL, assemblyName.CodeBase);
                }
                return assemblyFolder;
            }
        }
    }
}
