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
        const string PackageTagFilter = "Bonsai";
        const string GalleryDirectory = "Gallery";
        const string ExtensionsDirectory = "Extensions";
        const string BuildDirectory = "build";
        const string BinDirectory = "bin";
        const string DebugDirectory = "debug";
        const string BonsaiExtension = ".bonsai";
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

            var galleryPath = Path.Combine(bootstrapperDirectory, GalleryDirectory);
            var galleryPackageSource = new PackageSource(galleryPath);
            galleryRepository = new SourceRepository(galleryPackageSource, Repository.Provider.GetCoreV3());
            bootstrapperFramework = projectFramework;
        }

        string GetRelativePath(string path)
        {
            var editorRoot = packageManager.LocalRepository.PackageSource.Source;
            var rootUri = new Uri(editorRoot);
            var pathUri = new Uri(path);
            var relativeUri = rootUri.MakeRelativeUri(pathUri);
            return PathUtility.GetPathWithDirectorySeparator(relativeUri.ToString());
        }

        static bool IsTaggedPackage(PackageReaderBase package)
        {
            var tags = package.NuspecReader.GetTags();
            return tags != null && tags.Contains(PackageTagFilter);
        }

        static string ResolvePlatformNameAlias(string name)
        {
            switch (name)
            {
                case "x64":
                case "amd64":
                case "em64t":
                case "intel64":
                case "x86-64":
                case "x86_64":
                    return "x64";
                case "win32":
                case "x86":
                case "ia32":
                case "386":
                    return "x86";
                default:
                    return string.Empty;
            }
        }

        static string ResolvePathPlatformName(string path)
        {
            var platformName = string.Empty;
            var components = path.Split(DirectorySeparators, StringSplitOptions.RemoveEmptyEntries);
            components = Array.ConvertAll(components, name => name.ToLower());

            if (components.Length > 3 && components[2] == BinDirectory &&
                Array.FindIndex(components, name => name == DebugDirectory) < 0)
            {
                for (int i = 3; i < components.Length; i++)
                {
                    platformName = ResolvePlatformNameAlias(components[i]);
                    if (!string.IsNullOrEmpty(platformName)) break;
                }
            }

            return platformName;
        }

        static IEnumerable<string> GetAssemblyLocations(NuGetFramework projectFramework, PackageReaderBase package)
        {
            var nearestFramework = package.GetItems(BuildDirectory).GetNearest(projectFramework);
            if (nearestFramework == null) return Enumerable.Empty<string>();

            return from file in nearestFramework.Items
                   where Path.GetExtension(file) == AssemblyExtension &&
                         !string.IsNullOrEmpty(ResolvePathPlatformName(file))
                   select PathUtility.GetPathWithDirectorySeparator(file);
        }

        static IEnumerable<LibraryFolder> GetLibraryFolders(PackageReaderBase package, string installPath)
        {
            var nativeFramework = package.GetItems(BuildDirectory).FirstOrDefault(
                frameworkGroup => NuGetFramework.FrameworkNameComparer.Equals(frameworkGroup.TargetFramework, NativeFramework));
            if (nativeFramework == null) return Enumerable.Empty<LibraryFolder>();

            return from file in nativeFramework.Items
                   group file by Path.GetDirectoryName(file) into folder
                   let platform = ResolvePathPlatformName(folder.Key)
                   where !string.IsNullOrWhiteSpace(platform)
                   select new LibraryFolder(Path.Combine(installPath, folder.Key), platform);
        }

        static IEnumerable<string> GetCompatibleAssemblyReferences(NuGetFramework projectFramework, PackageReaderBase package)
        {
            var nearestFramework = package.GetReferenceItems().GetNearest(projectFramework);
            if (nearestFramework == null) return Enumerable.Empty<string>();
            return nearestFramework.Items.Select(PathUtility.GetPathWithDirectorySeparator);
        }

        void RegisterAssemblyLocations(PackageReaderBase package, string installPath, string relativePath, bool addReferences)
        {
            var assemblyLocations = GetAssemblyLocations(bootstrapperFramework, package);
            RegisterAssemblyLocations(assemblyLocations, installPath, relativePath, addReferences);
        }

        void RegisterAssemblyLocations(IEnumerable<string> assemblyLocations, string installPath, string relativePath, bool addReferences)
        {
            foreach (var path in assemblyLocations)
            {
                var assemblyFile = Path.Combine(installPath, path);
                var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
                var assemblyLocation = Path.Combine(relativePath, path);
                var assemblyLocationKey = Tuple.Create(assemblyName.Name, assemblyName.ProcessorArchitecture);
                if (!packageConfiguration.AssemblyLocations.Contains(assemblyLocationKey))
                {
                    packageConfiguration.AssemblyLocations.Add(assemblyName.Name, assemblyName.ProcessorArchitecture, assemblyLocation);
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
            var assemblyLocations = GetAssemblyLocations(bootstrapperFramework, package);
            RemoveAssemblyLocations(assemblyLocations, installPath, removeReference);
        }

        void RemoveAssemblyLocations(IEnumerable<string> assemblyLocations, string installPath, bool removeReference)
        {
            foreach (var path in assemblyLocations)
            {
                var assemblyFile = Path.Combine(installPath, path);
                var location = packageConfiguration.AssemblyLocations.FirstOrDefault(item => item.Location == assemblyFile);
                if (location != null)
                {
                    packageConfiguration.AssemblyLocations.Remove(Tuple.Create(location.AssemblyName, location.ProcessorArchitecture));
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

        class PackageConfigurationPlugin : PackageManagerPlugin
        {
            public PackageConfigurationPlugin(PackageConfigurationUpdater owner)
            {
                Owner = owner;
            }

            PackageConfigurationUpdater Owner { get; set; }

            public override async Task<bool> OnPackageInstallingAsync(PackageIdentity package, NuGetFramework projectFramework, PackageReaderBase packageReader, string installPath)
            {
                var entryPoint = package.Id + BonsaiExtension;
                var nearestFrameworkGroup = packageReader.GetContentItems().GetNearest(Owner.bootstrapperFramework);
                var executablePackage = nearestFrameworkGroup?.Items.Any(file => PathUtility.GetRelativePath(ContentFolder, file) == entryPoint);
                if (executablePackage.GetValueOrDefault())
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
                    var pivots = OverlayHelper.FindPivots(packageReader, installPath).ToArray();
                    if (pivots.Length > 0)
                    {
                        PathUtility.EnsureParentDirectory(Path.Combine(installPath, package.Id));
                        var overlayVersion = OverlayHelper.FindOverlayVersion(packageReader);
                        var overlayManager = OverlayHelper.CreateOverlayManager(Owner.packageManager, installPath);
                        overlayManager.Logger = Owner.packageManager.Logger;
                        try
                        {
                            foreach (var pivot in pivots)
                            {
                                var pivotIdentity = new PackageIdentity(pivot, overlayVersion);
                                var pivotPackage = await overlayManager.InstallPackageAsync(pivotIdentity, projectFramework, ignoreDependencies: true, CancellationToken.None);
                                if (pivotPackage == null) throw new InvalidOperationException(string.Format("The package '{0}' could not be found.", pivot));
                            }
                        }
                        catch
                        {
                            foreach (var pivot in pivots)
                            {
                                var pivotIdentity = new PackageIdentity(pivot, overlayVersion);
                                await overlayManager.UninstallPackageAsync(pivotIdentity, projectFramework, removeDependencies: false, CancellationToken.None);
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
                var taggedPackage = IsTaggedPackage(packageReader);
                var relativePath = Owner.GetRelativePath(installPath);
                if (!packageConfiguration.Packages.Contains(package.Id))
                {
                    packageConfiguration.Packages.Add(package.Id, package.Version.ToString());
                }
                else packageConfiguration.Packages[package.Id].Version = package.Version.ToString();

                Owner.AddContentFolders(installPath, ExtensionsDirectory);
                Owner.RegisterLibraryFolders(packageReader, relativePath);
                Owner.RegisterAssemblyLocations(packageReader, installPath, relativePath, false);
                var pivots = OverlayHelper.FindPivots(packageReader, installPath).ToArray();
                if (pivots.Length > 0)
                {
                    var overlayManager = OverlayHelper.CreateOverlayManager(Owner.packageManager, installPath);
                    foreach (var pivot in pivots)
                    {
                        var pivotPackage = overlayManager.LocalRepository.FindLocalPackage(pivot);
                        using var pivotReader = pivotPackage.GetReader();
                        Owner.RegisterLibraryFolders(pivotReader, relativePath);
                        Owner.RegisterAssemblyLocations(pivotReader, installPath, relativePath, false);
                    }
                }

                var assemblyLocations = GetCompatibleAssemblyReferences(Owner.bootstrapperFramework, packageReader);
                Owner.RegisterAssemblyLocations(assemblyLocations, installPath, relativePath, taggedPackage);
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
                var taggedPackage = IsTaggedPackage(packageReader);
                var relativePath = Owner.GetRelativePath(installPath);
                Owner.packageConfiguration.Packages.Remove(package.Id);

                Owner.RemoveContentFolders(packageReader, installPath, ExtensionsDirectory);
                Owner.RemoveLibraryFolders(packageReader, relativePath);
                Owner.RemoveAssemblyLocations(packageReader, relativePath, false);
                var pivots = OverlayHelper.FindPivots(packageReader, installPath).ToArray();
                if (pivots.Length > 0)
                {
                    var overlayManager = OverlayHelper.CreateOverlayManager(Owner.packageManager, installPath);
                    foreach (var pivot in pivots)
                    {
                        var pivotPackage = overlayManager.LocalRepository.FindLocalPackage(pivot);
                        if (pivotPackage != null)
                        {
                            using var pivotReader = pivotPackage.GetReader();
                            Owner.RemoveLibraryFolders(pivotReader, relativePath);
                            Owner.RemoveAssemblyLocations(pivotReader, relativePath, false);
                        }
                    }
                }

                var assemblyLocations = GetCompatibleAssemblyReferences(Owner.bootstrapperFramework, packageReader);
                Owner.RemoveAssemblyLocations(assemblyLocations, relativePath, taggedPackage);
                Owner.packageConfiguration.Save();

                if (pivots.Length > 0)
                {
                    var overlayVersion = OverlayHelper.FindOverlayVersion(packageReader);
                    var overlayManager = OverlayHelper.CreateOverlayManager(Owner.packageManager, installPath);
                    overlayManager.Logger = Owner.packageManager.Logger;
                    foreach (var pivot in pivots)
                    {
                        var pivotIdentity = new PackageIdentity(pivot, overlayVersion);
                        await overlayManager.UninstallPackageAsync(pivotIdentity, projectFramework, removeDependencies: false, CancellationToken.None);
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
