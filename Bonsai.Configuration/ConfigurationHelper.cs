using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Configuration
{
    public static class ConfigurationHelper
    {
        const string RepositoryPath = "Packages";
        const string PathEnvironmentVariable = "PATH";
        const string DefaultConfigurationFileName = "Bonsai.config";

        static string GetEnvironmentPlatform()
        {
            return Environment.Is64BitProcess ? "x64" : "x86";
        }

        static string GetDefaultConfigurationFilePath()
        {
            var configurationRoot = GetConfigurationRoot();
            return Path.Combine(configurationRoot, DefaultConfigurationFileName);
        }

        static void AddLibraryPath(string path)
        {
            var currentPath = Environment.GetEnvironmentVariable(PathEnvironmentVariable);
            if (!currentPath.Contains(path))
            {
                currentPath = string.Join(new string(Path.PathSeparator, 1), path, currentPath);
                Environment.SetEnvironmentVariable(PathEnvironmentVariable, currentPath);
            }
        }

        public static string GetConfigurationRoot(PackageConfiguration configuration = null)
        {
            return configuration == null || string.IsNullOrWhiteSpace(configuration.ConfigurationFile)
                ? AppDomain.CurrentDomain.BaseDirectory
                : Path.GetDirectoryName(configuration.ConfigurationFile);
        }

        public static string GetAssemblyLocation(this PackageConfiguration configuration, string assemblyName)
        {
            var msilAssembly = Tuple.Create(assemblyName, ProcessorArchitecture.MSIL);
            if (configuration.AssemblyLocations.Contains(msilAssembly))
            {
                return configuration.AssemblyLocations[msilAssembly].Location;
            }

            var architectureSpecificAssembly = Tuple.Create(assemblyName, Environment.Is64BitProcess ? ProcessorArchitecture.Amd64 : ProcessorArchitecture.X86);
            if (configuration.AssemblyLocations.Contains(architectureSpecificAssembly))
            {
                return configuration.AssemblyLocations[architectureSpecificAssembly].Location;
            }

            return null;
        }

        public static IDictionary<string, PackageReference> GetPackageReferenceMap(this PackageConfiguration configuration)
        {
            var baseDirectory = Path.GetDirectoryName(configuration.ConfigurationFile);
            var rootDirectory = Path.Combine(baseDirectory, RepositoryPath);
            var pathResolver = new PackagePathResolver(rootDirectory);
            var packageMap = new Dictionary<string, PackageReference>();
            foreach (var package in configuration.Packages)
            {
                var identity = new PackageIdentity(package.Id, NuGetVersion.Parse(package.Version));
                var packagePath = pathResolver.GetPackageDirectoryName(identity);
                if (packagePath != null)
                {
                    packageMap.Add(packagePath, package);
                }
            }

            return packageMap;
        }

        public static IEnumerable<PackageReference> GetAssemblyPackageReferences(
            this PackageConfiguration configuration,
            IEnumerable<string> assemblyNames,
            IDictionary<string, PackageReference> packageMap)
        {
            var dependencies = new List<PackageReference>();
            foreach (var assemblyName in assemblyNames)
            {
                var assemblyLocation = GetAssemblyLocation(configuration, assemblyName);
                if (assemblyLocation != null)
                {
                    var pathElements = assemblyLocation.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    if (pathElements.Length > 1 && pathElements[0] == RepositoryPath)
                    {
                        if (packageMap.TryGetValue(pathElements[1], out PackageReference package))
                        {
                            dependencies.Add(package);
                        }
                    }
                }
            }

            return dependencies;
        }

        public static void SetAssemblyResolve(PackageConfiguration configuration)
        {
            var platform = GetEnvironmentPlatform();
            var configurationRoot = GetConfigurationRoot(configuration);
            foreach (var libraryFolder in configuration.LibraryFolders)
            {
                if (libraryFolder.Platform == platform)
                {
                    var libraryPath = libraryFolder.Path;
                    if (!Path.IsPathRooted(libraryPath))
                    {
                        libraryPath = Path.Combine(configurationRoot, libraryPath);
                    }
                    AddLibraryPath(libraryPath);
                }
            }

            Dictionary<string, Assembly> assemblyLoadCache = null;
            ResolveEventHandler assemblyResolveHandler = (sender, args) =>
            {
                var assemblyName = new AssemblyName(args.Name).Name;
                var assemblyLocation = GetAssemblyLocation(configuration, assemblyName);
                if (assemblyLocation != null)
                {
                    if (assemblyLocation.StartsWith(Uri.UriSchemeFile) && Uri.TryCreate(assemblyLocation, UriKind.Absolute, out Uri uri))
                    {
                        assemblyLoadCache ??= new Dictionary<string, Assembly>();
                        if (!assemblyLoadCache.TryGetValue(uri.LocalPath, out Assembly assembly))
                        {
                            var assemblyBytes = File.ReadAllBytes(uri.LocalPath);
                            assembly = Assembly.Load(assemblyBytes);
                            assemblyLoadCache.Add(uri.LocalPath, assembly);
                        }
                        return assembly;
                    }

                    if (!Path.IsPathRooted(assemblyLocation))
                    {
                        assemblyLocation = Path.Combine(configurationRoot, assemblyLocation);
                    }

                    if (File.Exists(assemblyLocation))
                    {
                        return Assembly.LoadFrom(assemblyLocation);
                    }
                }

                return null;
            };

            AppDomain.CurrentDomain.AssemblyResolve += assemblyResolveHandler;
        }

        public static void SetAssemblyResolve()
        {
            var configuration = Load();
            SetAssemblyResolve(configuration);
        }

        public static PackageConfiguration Load(string fileName = null)
        {
            if (fileName == null) fileName = GetDefaultConfigurationFilePath();
            if (!File.Exists(fileName)) return new PackageConfiguration();
            var serializer = new XmlSerializer(typeof(PackageConfiguration));
            using (var reader = XmlReader.Create(fileName))
            {
                var configuration = (PackageConfiguration)serializer.Deserialize(reader);
                configuration.ConfigurationFile = fileName;
                return configuration;
            }
        }

        public static void Save(this PackageConfiguration configuration, string fileName = null)
        {
            if (fileName == null) fileName = configuration.ConfigurationFile ?? GetDefaultConfigurationFilePath();
            var serializer = new XmlSerializer(typeof(PackageConfiguration));
            using (var writer = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true }))
            {
                serializer.Serialize(writer, configuration);
                configuration.ConfigurationFile = fileName;
            }
        }

        public static void RegisterPath(this PackageConfiguration configuration, string path)
        {
            if (!Directory.Exists(path)) return;
            var platform = GetEnvironmentPlatform();
            if (!configuration.LibraryFolders.Contains(path))
            {
                configuration.LibraryFolders.Add(path, platform);
            }
            else if (configuration.LibraryFolders[path].Platform != platform)
            {
                var message = string.Format("The library path '{0}' is already registered for a different platform.", path);
                throw new InvalidOperationException(message);
            }

            foreach (var assemblyFile in Directory.GetFiles(path, "*.dll"))
            {
                AssemblyName assemblyName;
                try { assemblyName = AssemblyName.GetAssemblyName(assemblyFile); }
                catch (BadImageFormatException) { continue; }
                catch (IOException) { continue; }

                var locationKey = Tuple.Create(assemblyName.Name, assemblyName.ProcessorArchitecture);
                if (!configuration.AssemblyLocations.Contains(locationKey))
                {
                    configuration.AssemblyReferences.Add(assemblyName.Name);
                    configuration.AssemblyLocations.Add(assemblyName.Name, assemblyName.ProcessorArchitecture, assemblyFile);
                }
            }
        }
    }
}
