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
        const string DefaultProbingPath = "Packages";
        const string DefaultConfigurationFileName = "Bonsai.config";

        static string GetEnvironmentPlatform()
        {
            return Environment.Is64BitProcess ? "x64" : "x86";
        }

        static string GetDefaultConfigurationFilePath()
        {
            var configurationRoot = Path.GetDirectoryName(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            return Path.Combine(configurationRoot, DefaultConfigurationFileName);
        }

        static void AddLibraryPath(string path)
        {
            path = Path.GetFullPath(path);
            var currentPath = Environment.GetEnvironmentVariable(PathEnvironmentVariable);
            currentPath = string.Join(new string(Path.PathSeparator, 1), currentPath, path);
            Environment.SetEnvironmentVariable(PathEnvironmentVariable, currentPath);
        }

        public static void SetAssemblyResolve(PackageConfiguration configuration)
        {
            var configurationFile = configuration.ConfigurationFile;
            var configurationRoot = string.IsNullOrWhiteSpace(configurationFile)
                ? string.Empty
                : Path.GetDirectoryName(configuration.ConfigurationFile);

            var platform = GetEnvironmentPlatform();
            foreach (var libraryFolder in configuration.LibraryFolders)
            {
                if (libraryFolder.Platform == platform)
                {
                    var libraryPath = Path.Combine(configurationRoot, libraryFolder.Path);
                    AddLibraryPath(libraryPath);
                }
            }

            ResolveEventHandler assemblyResolveHandler = (sender, args) =>
            {
                var assemblyName = new AssemblyName(args.Name).Name;
                if (configuration.AssemblyLocations.Contains(assemblyName))
                {
                    var assemblyLocation = configuration.AssemblyLocations[assemblyName].Location;
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
            var platform = GetEnvironmentPlatform();
            configuration.LibraryFolders.Add(path, platform);
            foreach (var assemblyFile in Directory.GetFiles(path, "*.dll"))
            {
                var assemblyName = Path.GetFileNameWithoutExtension(assemblyFile);
                if (!configuration.AssemblyLocations.Contains(assemblyName))
                {
                    configuration.AssemblyReferences.Add(assemblyName);
                    configuration.AssemblyLocations.Add(assemblyName, assemblyFile);
                }
            }
        }
    }
}
