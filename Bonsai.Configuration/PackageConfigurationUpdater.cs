using Bonsai.Configuration.Properties;
using Bonsai.NuGet;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Configuration
{
    public class PackageConfigurationUpdater : IDisposable
    {
        const string BinDirectory = "bin";
        const string DebugDirectory = "debug";
        const string AssemblyExtension = ".dll";
        const string OldExtension = ".old";

        readonly string bootstrapperExePath;
        readonly string bootstrapperDirectory;
        readonly string bootstrapperPackageId;
        readonly NuGetVersion bootstrapperVersion;
        readonly NuGetFramework bootstrapperFramework;
        readonly IPackageManager packageManager;
        readonly SourceRepository galleryRepository;
        readonly PackageConfiguration packageConfiguration;
        readonly PackageConfigurationPlugin configurationPlugin;
        static readonly string ContentFolder = PathUtility.EnsureTrailingSlash(PackagingConstants.Folders.Content);
        static readonly char[] DirectorySeparators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        static readonly NuGetFramework NativeFramework = NuGetFramework.ParseFrameworkName("native,Version=v0.0", DefaultFrameworkNameProvider.Instance);
        static readonly NuGetFramework WindowsFramework = new NuGetFramework(FrameworkConstants.FrameworkIdentifiers.Windows, FrameworkConstants.EmptyVersion);
        static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;

        public PackageConfigurationUpdater(NuGetFramework projectFramework, PackageConfiguration configuration, IPackageManager manager, string bootstrapperPath = null, PackageIdentity bootstrapperName = null)
        {
            packageManager = manager ?? throw new ArgumentNullException(nameof(manager));
            packageConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            bootstrapperFramework = projectFramework ?? throw new ArgumentNullException(nameof(projectFramework));
            bootstrapperExePath = bootstrapperPath ?? string.Empty;
            bootstrapperDirectory = Path.GetDirectoryName(bootstrapperExePath);
            bootstrapperPackageId = bootstrapperName != null ? bootstrapperName.Id : string.Empty;
            bootstrapperVersion = bootstrapperName?.Version;
            configurationPlugin = new PackageConfigurationPlugin(this);
            packageManager.PackageManagerPlugins.Add(configurationPlugin);

            var galleryPath = Path.Combine(bootstrapperDirectory, Constants.GalleryDirectory);
            var galleryPackageSource = new PackageSource(galleryPath);
            galleryRepository = new SourceRepository(galleryPackageSource, Repository.Provider.GetCoreV3());
            NormalizePathSeparators(packageConfiguration);
        }

        string GetRelativePath(string path)
        {
            var editorRoot = packageManager.LocalRepository.PackageSource.Source;
            var rootUri = new Uri(editorRoot);
            var pathUri = new Uri(path);
            var relativeUri = rootUri.MakeRelativeUri(pathUri);
            return PathUtility.GetPathWithDirectorySeparator(relativeUri.ToString());
        }

        static string CombinePath(string path1, string path2)
        {
            return PathUtility.GetPathWithForwardSlashes(Path.Combine(path1, path2));
        }

        static void NormalizePathSeparators(PackageConfiguration configuration)
        {
            foreach (var assemblyLocation in configuration.AssemblyLocations)
            {
                assemblyLocation.Location = PathUtility.GetPathWithForwardSlashes(assemblyLocation.Location);
            }

            // cannot normalize in place since path is collection key
            var libraryFolders = configuration.LibraryFolders.ToArray();
            configuration.LibraryFolders.Clear();
            foreach (var folder in libraryFolders)
            {
                folder.Path = PathUtility.GetPathWithForwardSlashes(folder.Path);
                configuration.LibraryFolders.Add(folder);
            }
        }

        static ProcessorArchitecture ResolveArchitectureAlias(string name)
        {
            switch (name)
            {
                case "x64":
                case "amd64":
                case "em64t":
                case "intel64":
                case "x86-64":
                case "x86_64":
                    return ProcessorArchitecture.Amd64;
                case "win32":
                case "x86":
                case "ia32":
                case "386":
                    return ProcessorArchitecture.X86;
                default:
                    return ProcessorArchitecture.None;
            }
        }

        static ProcessorArchitecture ResolvePathArchitecture(string path)
        {
            var architecture = ProcessorArchitecture.None;
            var components = path.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
            components = Array.ConvertAll(components, name => name.ToLower());

            if (components.Length > 3 && components[2] == BinDirectory &&
                Array.FindIndex(components, name => name == DebugDirectory) < 0)
            {
                for (int i = 3; i < components.Length; i++)
                {
                    architecture = ResolveArchitectureAlias(components[i]);
                    if (architecture != ProcessorArchitecture.None) break;
                }
            }

            return architecture;
        }

        static IEnumerable<IGrouping<ProcessorArchitecture, string>> GetArchitectureSpecificAssemblyLocations(NuGetFramework projectFramework, PackageReaderBase package)
        {
            var nearestFramework = package.GetItems(PackagingConstants.Folders.Build).GetNearest(projectFramework);
            if (nearestFramework == null) return Enumerable.Empty<IGrouping<ProcessorArchitecture, string>>();

            return from file in nearestFramework.Items
                   where Path.GetExtension(file) == AssemblyExtension
                   let architecture = ResolvePathArchitecture(file)
                   where architecture != ProcessorArchitecture.None
                   group PathUtility.GetPathWithForwardSlashes(file) by architecture;
        }

        static IEnumerable<LibraryFolder> GetLibraryFolders(PackageReaderBase package, string installPath)
        {
            var buildFolders = GetBuildLibraryFolders(package, installPath);
            var runtimeFolders = GetRuntimeLibraryFolders(package, installPath);
            return buildFolders.Concat(runtimeFolders);
        }

        static IEnumerable<LibraryFolder> GetBuildLibraryFolders(PackageReaderBase package, string installPath)
        {
            var nativeFramework = package.GetItems(PackagingConstants.Folders.Build).FirstOrDefault(
                frameworkGroup => NuGetFramework.FrameworkNameComparer.Equals(frameworkGroup.TargetFramework, NativeFramework));
            if (nativeFramework == null) return Enumerable.Empty<LibraryFolder>();

            return from file in nativeFramework.Items
                   group file by Path.GetDirectoryName(file) into folder
                   let architecture = ResolvePathArchitecture(folder.Key)
                   where architecture != ProcessorArchitecture.None
                   select new LibraryFolder(
                       CombinePath(installPath, folder.Key),
                       architecture == ProcessorArchitecture.X86 ? "x86" : "x64");
        }

        static IEnumerable<LibraryFolder> GetRuntimeLibraryFolders(PackageReaderBase package, string installPath)
        {
            return from frameworkGroup in package.GetItems(PackagingConstants.Folders.Runtimes)
                   where NuGetFramework.FrameworkNameComparer.Equals(frameworkGroup.TargetFramework, WindowsFramework)
                   let platform = frameworkGroup.TargetFramework.Profile
                   where !string.IsNullOrWhiteSpace(platform)
                   from file in frameworkGroup.Items
                   group file by new { platform, path = Path.GetDirectoryName(file) } into folder
                   select new LibraryFolder(CombinePath(installPath, folder.Key.path), folder.Key.platform);
        }

        static IEnumerable<string> GetCompatibleAssemblyReferences(NuGetFramework projectFramework, PackageReaderBase package)
        {
            var nearestFramework = package.GetReferenceItems().GetNearest(projectFramework);
            if (nearestFramework == null) return Enumerable.Empty<string>();
            return nearestFramework.Items.Select(PathUtility.GetPathWithForwardSlashes);
        }

        void RegisterAssemblyLocations(PackageReaderBase package, string installPath, string relativePath, bool addReferences)
        {
            var platformSpecificLocations = GetArchitectureSpecificAssemblyLocations(bootstrapperFramework, package);
            foreach (var assemblyLocations in platformSpecificLocations)
            {
                RegisterAssemblyLocations(assemblyLocations, installPath, relativePath, addReferences, assemblyLocations.Key);
            }
        }

        void RegisterAssemblyLocations(
            IEnumerable<string> assemblyLocations,
            string installPath,
            string relativePath,
            bool addReferences,
            ProcessorArchitecture processorArchitecture = ProcessorArchitecture.None)
        {
            foreach (var path in assemblyLocations)
            {
                var assemblyFile = CombinePath(installPath, path);
                var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
                if (processorArchitecture == ProcessorArchitecture.None)
                {
#if NET7_0_OR_GREATER
                    // Support for ProcessorArchitecture was removed in NET7 so assume MSIL for now
                    processorArchitecture = ProcessorArchitecture.MSIL;
#else
                    processorArchitecture = assemblyName.ProcessorArchitecture;
#endif
                }

                var assemblyLocation = CombinePath(relativePath, path);
                var assemblyLocationKey = (assemblyName.Name, processorArchitecture);
                if (!packageConfiguration.AssemblyLocations.Contains(assemblyLocationKey))
                {
                    packageConfiguration.AssemblyLocations.Add(assemblyName.Name, processorArchitecture, assemblyLocation);
                }
                else if (packageConfiguration.AssemblyLocations[assemblyLocationKey].Location != assemblyLocation)
                {
                    throw new InvalidOperationException(string.Format(Resources.AssemblyReferenceLocationMismatchException, assemblyLocationKey));
                }

                if (addReferences && !packageConfiguration.AssemblyReferences.Contains(assemblyName.Name))
                {
                    packageConfiguration.AssemblyReferences.Add(assemblyName.Name);
                }
            }
        }

        void RemoveAssemblyLocations(PackageReaderBase package, string installPath, bool removeReference)
        {
            var platformSpecificLocations = GetArchitectureSpecificAssemblyLocations(bootstrapperFramework, package);
            foreach (var assemblyLocations in platformSpecificLocations)
            {
                RemoveAssemblyLocations(assemblyLocations, installPath, removeReference);
            }
        }

        void RemoveAssemblyLocations(IEnumerable<string> assemblyLocations, string installPath, bool removeReference)
        {
            foreach (var path in assemblyLocations)
            {
                var assemblyFile = CombinePath(installPath, path);
                var location = packageConfiguration.AssemblyLocations.FirstOrDefault(item => item.Location == assemblyFile);
                if (location != null)
                {
                    packageConfiguration.AssemblyLocations.Remove((location.AssemblyName, location.ProcessorArchitecture));
                    if (removeReference)
                    {
                        packageConfiguration.AssemblyReferences.Remove(location.AssemblyName);
                    }
                }
            }
        }

        void RegisterLibraryFolders(PackageReaderBase package, string installPath)
        {
            foreach (var folder in GetLibraryFolders(package, installPath))
            {
                if (!packageConfiguration.LibraryFolders.Contains(folder.Path))
                {
                    packageConfiguration.LibraryFolders.Add(folder);
                }
                else if (packageConfiguration.LibraryFolders[folder.Path].Platform != folder.Platform)
                {
                    throw new InvalidOperationException(string.Format(Resources.LibraryFolderPlatformMismatchException, folder.Path));
                }
            }
        }

        void RemoveLibraryFolders(PackageReaderBase package, string installPath)
        {
            foreach (var folder in GetLibraryFolders(package, installPath))
            {
                packageConfiguration.LibraryFolders.Remove(folder.Path);
            }
        }

        static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.Equals(target.FullName, StringComparison.OrdinalIgnoreCase)) return;
            if (!target.Exists) Directory.CreateDirectory(target.FullName);

            foreach (var file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }

            foreach (var directory in source.GetDirectories())
            {
                var targetDirectory = target.CreateSubdirectory(directory.Name);
                CopyDirectory(directory, targetDirectory);
            }
        }

        void AddContentFolders(string installPath, string contentPath)
        {
            var contentDirectory = new DirectoryInfo(Path.Combine(installPath, PackagingConstants.Folders.Content, contentPath));
            if (contentDirectory.Exists)
            {
                var bootstrapperContent = new DirectoryInfo(Path.Combine(bootstrapperDirectory, contentPath));
                if (!bootstrapperContent.Exists) bootstrapperContent.Create();
                CopyDirectory(contentDirectory, bootstrapperContent);
            }
        }

        void RemoveEmptyDirectories(DirectoryInfo directory)
        {
            try
            {
                foreach (var subdirectory in directory.EnumerateDirectories())
                {
                    RemoveEmptyDirectories(subdirectory);
                }

                if (!directory.EnumerateFileSystemInfos().Any())
                {
                    try { directory.Delete(); }
                    catch (UnauthorizedAccessException) { } //best effort
                }
            }
            catch (DirectoryNotFoundException) { } //best effort
            catch (UnauthorizedAccessException) { } //best effort
        }

        void RemoveContentFolders(PackageReaderBase package, string installPath, string contentPath)
        {
            var contentDirectory = new DirectoryInfo(Path.Combine(installPath, PackagingConstants.Folders.Content, contentPath));
            if (contentDirectory.Exists)
            {
                var bootstrapperContent = new DirectoryInfo(Path.Combine(bootstrapperDirectory, contentPath));
                if (bootstrapperContent.Exists)
                {
                    foreach (var file in package.GetFiles(Path.Combine(PackagingConstants.Folders.Content, contentPath)))
                    {
                        var path = file.Split(DirectorySeparators, 2)[1];
                        var bootstrapperFilePath = Path.Combine(bootstrapperDirectory, path);
                        if (File.Exists(bootstrapperFilePath))
                        {
                            File.Delete(bootstrapperFilePath);
                        }
                    }

                    RemoveEmptyDirectories(bootstrapperContent);
                }
            }
        }

        static IEnumerable<string> GetAssemblyConfigFiles(NuGetFramework projectFramework, PackageReaderBase package, string installPath)
        {
            const string AssemblyConfigExtension = ".dll.config";
            var contentFolder = package.GetItems(PackagingConstants.Folders.ContentFiles).GetNearest(projectFramework) ??
                                package.GetItems(PackagingConstants.Folders.Content).GetNearest(projectFramework);
            if (contentFolder == null) return Enumerable.Empty<string>();
            return from file in contentFolder.Items
                   where file.EndsWith(AssemblyConfigExtension)
                   select Path.Combine(installPath, file);
        }

        void UpdateAssemblyConfigFiles(PackageReaderBase package, string installPath, Action<string, string> update)
        {
            var assemblyConfigFiles = GetAssemblyConfigFiles(bootstrapperFramework, package, installPath)
                .ToDictionary(configFile => Path.GetFileNameWithoutExtension(configFile));
            if (assemblyConfigFiles.Count > 0)
            {
                var assemblyLocations = GetCompatibleAssemblyReferences(bootstrapperFramework, package);
                foreach (var path in assemblyLocations)
                {
                    var assemblyName = Path.GetFileName(path);
                    if (assemblyConfigFiles.TryGetValue(assemblyName, out string configFilePath))
                    {
                        var configFileName = Path.GetFileName(configFilePath);
                        var assemblyConfigFilePath = Path.GetDirectoryName(path);
                        assemblyConfigFilePath = Path.Combine(installPath, assemblyConfigFilePath, configFileName);
                        update(configFilePath, assemblyConfigFilePath);
                    }
                }
            }
        }

        void AddAssemblyConfigFiles(PackageReaderBase package, string installPath)
        {
            UpdateAssemblyConfigFiles(package, installPath, File.Copy);
        }

        void RemoveAssemblyConfigFiles(PackageReaderBase package, string installPath)
        {
            UpdateAssemblyConfigFiles(package, installPath, (_, configFilePath) => File.Delete(configFilePath));
        }

        class PackageConfigurationPlugin : PackageManagerPlugin
        {
            public PackageConfigurationPlugin(PackageConfigurationUpdater owner)
            {
                Owner = owner;
            }

            PackageConfigurationUpdater Owner { get; set; }

            public override async Task<bool> OnPackageInstallingAsync(PackageIdentity package, NuGetFramework projectFramework, PackageReaderBase packageReader, string installPath)
            {
                if (packageReader.IsExecutablePackage(package, projectFramework))
                {
                    var packageFolder = Path.GetDirectoryName(packageReader.GetNuspecFile());
                    var resolver = new VersionFolderPathResolver(packageFolder, isLowercase: false);
                    var nupkgFileName = resolver.GetPackageFileName(package.Id, package.Version);
                    var nupkgFilePath = Path.Combine(packageFolder, nupkgFileName);
                    var localPackage = Owner.galleryRepository.GetLocalPackage(package);
                    if (localPackage == null)
                    {
                        var targetFilePath = Path.Combine(Owner.galleryRepository.PackageSource.Source, Path.GetFileName(nupkgFileName));
                        PathUtility.EnsureParentDirectory(targetFilePath);
                        File.Copy(nupkgFilePath, targetFilePath);
                    }
                    return false;
                }
                else
                {
                    var pivots = OverlayHelper.FindPivots(package, packageReader).ToArray();
                    if (pivots.Length > 0)
                    {
                        PathUtility.EnsureParentDirectory(Path.Combine(installPath, package.Id));
                        var overlayManager = OverlayHelper.CreateOverlayManager(Owner.packageManager, installPath);
                        overlayManager.Logger = Owner.packageManager.Logger;
                        try
                        {
                            foreach (var pivot in pivots)
                            {
                                var pivotPackage = await overlayManager.InstallPackageAsync(pivot, projectFramework, ignoreDependencies: true, CancellationToken.None);
                                if (pivotPackage == null) throw new InvalidOperationException(string.Format("The package '{0}' could not be found.", pivot));
                            }
                        }
                        catch
                        {
                            foreach (var pivot in pivots)
                            {
                                await overlayManager.UninstallPackageAsync(pivot, projectFramework, removeDependencies: false, CancellationToken.None);
                            }
                            throw;
                        }
                    }

                    return true;
                }
            }

            public override Task OnPackageInstalledAsync(PackageIdentity package, NuGetFramework projectFramework, PackageReaderBase packageReader, string installPath)
            {
                var packageConfiguration = Owner.packageConfiguration;
                var addReferences = packageReader.IsLibraryPackage();
                var relativePath = Owner.GetRelativePath(installPath);
                if (!packageConfiguration.Packages.Contains(package.Id))
                {
                    packageConfiguration.Packages.Add(package.Id, package.Version.ToString());
                }
                else packageConfiguration.Packages[package.Id].Version = package.Version.ToString();

                Owner.AddContentFolders(installPath, Constants.ExtensionsDirectory);
                Owner.RegisterLibraryFolders(packageReader, relativePath);
                Owner.RegisterAssemblyLocations(packageReader, installPath, relativePath, false);
                if (IsRunningOnMono) Owner.AddAssemblyConfigFiles(packageReader, installPath);
                var pivots = OverlayHelper.FindPivots(package, packageReader).ToArray();
                if (pivots.Length > 0)
                {
                    var overlayManager = OverlayHelper.CreateOverlayManager(Owner.packageManager, installPath);
                    foreach (var pivot in pivots)
                    {
                        var pivotPackage = overlayManager.LocalRepository.FindLocalPackage(pivot.Id);
                        using var pivotReader = pivotPackage.GetReader();
                        Owner.RegisterLibraryFolders(pivotReader, relativePath);
                        Owner.RegisterAssemblyLocations(pivotReader, installPath, relativePath, false);
                    }
                }

                // Reference assemblies should generally always be MSIL but for backwards compatibility
                // we allow the processor architecture to be set by the assembly for .NET Framework.
                // In future releases of the modern .NET bootstrapper we need to revisit this entirely
                // and ensure that none of these considerations impact on the Bonsai.config file,
                // most likely by removing all platform-specific paths and references. Runtime assembly
                // resolution is OS-specific and architecture-specific and should not be versioned together
                // with the package dependency graph.
                var assemblyLocations = GetCompatibleAssemblyReferences(projectFramework, packageReader);
                Owner.RegisterAssemblyLocations(assemblyLocations, installPath, relativePath, addReferences);
                packageConfiguration.Save();

                if (package.Id == Owner.bootstrapperPackageId && package.Version > Owner.bootstrapperVersion)
                {
                    var bootstrapperFileName = Path.GetFileName(Owner.bootstrapperExePath);
                    var bootstrapperFile = packageReader.GetFiles().FirstOrDefault(file => Path.GetFileName(file).Equals(bootstrapperFileName, StringComparison.OrdinalIgnoreCase));
                    if (bootstrapperFile == null)
                    {
                        throw new InvalidOperationException(Resources.BootstrapperMissingFromPackage);
                    }

                    var backupExePath = Owner.bootstrapperExePath + OldExtension;
                    File.Move(Owner.bootstrapperExePath, backupExePath);
                    UpdateFile(Owner.bootstrapperExePath, packageReader, bootstrapperFile);
                }

                return base.OnPackageInstalledAsync(package, projectFramework, packageReader, installPath);
            }

            public override async Task OnPackageUninstalledAsync(PackageIdentity package, NuGetFramework projectFramework, PackageReaderBase packageReader, string installPath)
            {
                var removeReferences = packageReader.IsLibraryPackage();
                var relativePath = Owner.GetRelativePath(installPath);
                Owner.packageConfiguration.Packages.Remove(package.Id);

                Owner.RemoveContentFolders(packageReader, installPath, Constants.ExtensionsDirectory);
                Owner.RemoveLibraryFolders(packageReader, relativePath);
                Owner.RemoveAssemblyLocations(packageReader, relativePath, false);
                if (IsRunningOnMono) Owner.RemoveAssemblyConfigFiles(packageReader, installPath);
                var pivots = OverlayHelper.FindPivots(package, packageReader).ToArray();
                if (pivots.Length > 0)
                {
                    var overlayManager = OverlayHelper.CreateOverlayManager(Owner.packageManager, installPath);
                    foreach (var pivot in pivots)
                    {
                        var pivotPackage = overlayManager.LocalRepository.FindLocalPackage(pivot.Id);
                        if (pivotPackage != null)
                        {
                            using var pivotReader = pivotPackage.GetReader();
                            Owner.RemoveLibraryFolders(pivotReader, relativePath);
                            Owner.RemoveAssemblyLocations(pivotReader, relativePath, false);
                        }
                    }
                }

                var assemblyLocations = GetCompatibleAssemblyReferences(projectFramework, packageReader);
                Owner.RemoveAssemblyLocations(assemblyLocations, relativePath, removeReferences);
                Owner.packageConfiguration.Save();

                if (pivots.Length > 0)
                {
                    var overlayManager = OverlayHelper.CreateOverlayManager(Owner.packageManager, installPath);
                    overlayManager.Logger = Owner.packageManager.Logger;
                    foreach (var pivot in pivots)
                    {
                        await overlayManager.UninstallPackageAsync(pivot, projectFramework, removeDependencies: false, CancellationToken.None);
                    }
                }
            }
        }

        static void UpdateFile(string path, PackageReaderBase packageReader, string file)
        {
            using Stream fromStream = packageReader.GetStream(file), toStream = File.Create(path);
            fromStream.CopyTo(toStream);
        }

        public void Dispose()
        {
            packageManager.PackageManagerPlugins.Remove(configurationPlugin);
        }
    }
}
