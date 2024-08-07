﻿using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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
            return RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant();
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

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                NativeMethods.AddDllDirectory(path);
            }
        }

        public static string GetConfigurationRoot(PackageConfiguration configuration = null)
        {
            return !string.IsNullOrWhiteSpace(configuration?.ConfigurationFile)
                ? Path.GetDirectoryName(configuration.ConfigurationFile)
                : AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetAssemblyLocation(this PackageConfiguration configuration, string assemblyName)
        {
            var msilAssembly = (assemblyName, ProcessorArchitecture.MSIL);
            if (configuration.AssemblyLocations.Contains(msilAssembly))
            {
                return configuration.AssemblyLocations[msilAssembly].Location;
            }

            var architectureSpecificAssembly = (assemblyName, Environment.Is64BitProcess ? ProcessorArchitecture.Amd64 : ProcessorArchitecture.X86);
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

        public static PackageReference GetAssemblyPackageReference(
            this PackageConfiguration configuration,
            string assemblyName,
            IDictionary<string, PackageReference> packageMap)
        {
            var assemblyLocation = GetAssemblyLocation(configuration, assemblyName);
            if (assemblyLocation != null)
            {
                var pathElements = assemblyLocation.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (pathElements.Length > 1 && pathElements[0] == RepositoryPath)
                {
                    if (packageMap.TryGetValue(pathElements[1], out PackageReference package))
                    {
                        return package;
                    }
                }
            }

            return null;
        }

        public static void SetAssemblyResolve(PackageConfiguration configuration, bool assemblyLock = true)
        {
            var platform = GetEnvironmentPlatform();
            var configurationRoot = GetConfigurationRoot(configuration);
            var libraryPaths = from folder in configuration.LibraryFolders
                               where folder.Platform == platform
                               select folder.Path;
            var extensionPaths = configuration.ExtensionFolders.Reverse();
            foreach (var libraryPath in libraryPaths.Concat(extensionPaths))
            {
                var path = libraryPath;
                if (!Path.IsPathRooted(path))
                {
                    path = Path.Combine(configurationRoot, path);
                }
                AddLibraryPath(path);
            }

            ConcurrentDictionary<string, Assembly> resolveCache = new();
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                var assemblyName = new AssemblyName(args.Name).Name;
                if (resolveCache.TryGetValue(assemblyName, out var assembly))
                    return assembly;

                var assemblyLocation = GetAssemblyLocation(configuration, assemblyName);
                if (assemblyLocation != null)
                {
                    if (assemblyLocation.StartsWith(Uri.UriSchemeFile) && Uri.TryCreate(assemblyLocation, UriKind.Absolute, out Uri uri))
                    {
                        var assemblyBytes = File.ReadAllBytes(uri.LocalPath);
                        assembly = Assembly.Load(assemblyBytes);
                    }
                    else
                    {
                        if (!Path.IsPathRooted(assemblyLocation))
                        {
                            assemblyLocation = Path.Combine(configurationRoot, assemblyLocation);
                        }

                        if (File.Exists(assemblyLocation))
                        {
                            if (!assemblyLock)
                            {
                                var assemblyBytes = File.ReadAllBytes(assemblyLocation);
                                assembly = Assembly.Load(assemblyBytes);
                            }
                            else assembly = Assembly.LoadFrom(assemblyLocation);
                        }
                    }
                }

                return resolveCache.GetOrAdd(assemblyName, assembly);
            };
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
            configuration.ExtensionFolders.Add(path);

            foreach (var assemblyFile in Directory.GetFiles(path, "*.dll"))
            {
                AssemblyName assemblyName;
                try { assemblyName = AssemblyName.GetAssemblyName(assemblyFile); }
                catch (BadImageFormatException) { continue; }
                catch (IOException) { continue; }

#if NET7_0_OR_GREATER
                // Support for ProcessorArchitecture was removed in NET7 so assume MSIL for now
                var processorArchitecture = ProcessorArchitecture.MSIL;
#else
                var processorArchitecture = assemblyName.ProcessorArchitecture;
#endif

                var locationKey = (assemblyName.Name, processorArchitecture);
                if (!configuration.AssemblyLocations.Contains(locationKey))
                {
                    configuration.AssemblyReferences.Add(assemblyName.Name);
                    configuration.AssemblyLocations.Add(assemblyName.Name, processorArchitecture, assemblyFile);
                }
            }
        }
    }
}
