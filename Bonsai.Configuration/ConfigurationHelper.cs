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

        static void AddLibraryPath(string path)
        {
            var currentPath = Environment.GetEnvironmentVariable(PathEnvironmentVariable);
            currentPath = string.Join(new string(Path.PathSeparator, 1), currentPath, path);
            Environment.SetEnvironmentVariable(PathEnvironmentVariable, currentPath);
        }

        public static IEnumerable<string> GetPackageFiles()
        {
            IEnumerable<string> packageFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll");
            if (!string.Equals(Path.GetFullPath(Environment.CurrentDirectory).TrimEnd('\\'),
                               Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory).TrimEnd('\\'),
                               StringComparison.InvariantCultureIgnoreCase))
            {
                var bonsaiAssemblyName = typeof(LoadableElement).Assembly.GetName();
                packageFiles = packageFiles.Concat(Directory
                    .GetFiles(Environment.CurrentDirectory, "*.dll")
                    .Where(fileName => AssemblyName.GetAssemblyName(fileName).FullName != bonsaiAssemblyName.FullName));
            }

            return packageFiles;
        }

        public static IDisposable SetAssemblyResolve()
        {
            return SetAssemblyResolve(AppDomain.CurrentDomain, string.Empty);
        }

        public static IDisposable SetAssemblyResolve(string privateBinPath)
        {
            return SetAssemblyResolve(AppDomain.CurrentDomain, privateBinPath);
        }

        public static IDisposable SetAssemblyResolve(AppDomain domain, string privateBinPath)
        {
            PackageConfiguration packageConfiguration;
            try { packageConfiguration = (PackageConfiguration)ConfigurationManager.GetSection(PackageConfiguration.SectionName) ?? new PackageConfiguration(); }
            catch (ConfigurationErrorsException) { packageConfiguration = new PackageConfiguration(); }

            foreach (PackageElement package in packageConfiguration.Packages)
            {
                if (!string.IsNullOrEmpty(package.LibraryPath))
                {
                    AddLibraryPath(package.LibraryPath);
                }
            }

            ResolveEventHandler assemblyResolveHandler = (sender, args) =>
            {
                var package = packageConfiguration.Packages[args.Name];
                if (package != null)
                {
                    return Assembly.LoadFrom(package.AssemblyLocation);
                }

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
    }
}
