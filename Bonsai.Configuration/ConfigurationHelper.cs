using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace Bonsai.Configuration
{
    public static class ConfigurationHelper
    {
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

        public static string GetAssemblyLocation(PackageConfiguration configuration, string assemblyName)
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
                    Uri uri;
                    if (assemblyLocation.StartsWith(Uri.UriSchemeFile) && Uri.TryCreate(assemblyLocation, UriKind.Absolute, out uri))
                    {
                        Assembly assembly;
                        assemblyLoadCache = assemblyLoadCache ?? new Dictionary<string, Assembly>();
                        if (!assemblyLoadCache.TryGetValue(uri.LocalPath, out assembly))
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
