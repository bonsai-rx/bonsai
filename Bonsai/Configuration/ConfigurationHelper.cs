using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.Reactive.Disposables;
using System.IO;

namespace Bonsai.Configuration
{
    public static class ConfigurationHelper
    {
        const string PathEnvironmentVariable = "PATH";
        const string DefaultProbingPath = "Packages";

        static void AddLibraryPath(string path)
        {
            path = Path.GetFullPath(path);
            var currentPath = Environment.GetEnvironmentVariable(PathEnvironmentVariable);
            currentPath = string.Join(new string(Path.PathSeparator, 1), currentPath, path);
            Environment.SetEnvironmentVariable(PathEnvironmentVariable, currentPath);
        }

        static IDisposable SetAssemblyResolve(AppDomain domain, string privateBinPath)
        {
            AddLibraryPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultProbingPath));
            AddLibraryPath(privateBinPath);

            ResolveEventHandler assemblyResolveHandler = (sender, args) =>
            {
                if (!string.IsNullOrEmpty(privateBinPath))
                {
                    var assemblyName = new AssemblyName(args.Name).Name;
                    var assemblyLocation = Path.Combine(privateBinPath, assemblyName) + ".dll";
                    if (File.Exists(assemblyLocation))
                    {
                        return Assembly.LoadFrom(assemblyLocation);
                    }
                }

                return null;
            };

            domain.AssemblyResolve += assemblyResolveHandler;
            return Disposable.Create(() => domain.AssemblyResolve -= assemblyResolveHandler);
        }

        public static IDisposable SetAssemblyResolve()
        {
            return SetAssemblyResolve(AppDomain.CurrentDomain, Environment.CurrentDirectory);
        }
    }
}
