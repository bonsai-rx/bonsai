using Bonsai.Configuration;
using Bonsai.Properties;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;

namespace Bonsai
{
    class PackageConfigurationUpdater : IDisposable
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
        readonly SemanticVersion bootstrapperVersion;
        readonly IPackageManager packageManager;
        readonly IPackageRepository galleryRepository;
        readonly PackageConfiguration packageConfiguration;
        static readonly char[] DirectorySeparators = new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        static readonly FrameworkName NativeFramework = new FrameworkName("native,Version=v0.0");
        static readonly IEnumerable<FrameworkName> SupportedFrameworks = new[]
        {
            new FrameworkName(".NETFramework,Version=v4.7.2"),
            new FrameworkName(".NETFramework,Version=v4.7.1"),
            new FrameworkName(".NETFramework,Version=v4.7"),
            new FrameworkName(".NETFramework,Version=v4.6.2"),
            new FrameworkName(".NETFramework,Version=v4.6.1"),
            new FrameworkName(".NETFramework,Version=v4.6"),
            new FrameworkName(".NETFramework,Version=v4.5"),
            new FrameworkName(".NETFramework,Version=v4.0"),
            new FrameworkName(".NETFramework,Version=v4.0,Profile=Client"),
            new FrameworkName(".NETFramework,Version=v3.5"),
            new FrameworkName(".NETFramework,Version=v3.5,Profile=Client"),
            new FrameworkName(".NETFramework,Version=v2.0")
        };

        public PackageConfigurationUpdater(PackageConfiguration configuration, IPackageManager manager, string bootstrapperPath = null, IPackageName bootstrapperName = null)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            packageManager = manager;
            packageConfiguration = configuration;
            bootstrapperExePath = bootstrapperPath ?? string.Empty;
            bootstrapperDirectory = Path.GetDirectoryName(bootstrapperExePath);
            bootstrapperPackageId = bootstrapperName != null ? bootstrapperName.Id : string.Empty;
            bootstrapperVersion = bootstrapperName != null ? bootstrapperName.Version : null;
            packageManager.PackageInstalling += packageManager_PackageInstalling;
            packageManager.PackageInstalled += packageManager_PackageInstalled;
            packageManager.PackageUninstalling += packageManager_PackageUninstalling;

            var galleryPath = Path.Combine(bootstrapperDirectory, GalleryDirectory);
            var galleryFileSystem = new PhysicalFileSystem(galleryPath);
            var galleryPathResolver = new GalleryPackagePathResolver(galleryPath);
            galleryRepository = new LocalPackageRepository(galleryPathResolver, galleryFileSystem);
        }

        string GetRelativePath(string path)
        {
            var editorRoot = packageManager.FileSystem.Root;
            var rootUri = new Uri(editorRoot);
            var pathUri = new Uri(path);
            var relativeUri = rootUri.MakeRelativeUri(pathUri);
            return relativeUri.ToString().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        }

        static bool IsTaggedPackage(IPackage package)
        {
            return package.Tags != null && package.Tags.Contains(PackageTagFilter);
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

        static IEnumerable<string> GetAssemblyLocations(IPackage package)
        {
            return from file in package.GetFiles(BuildDirectory)
                   where file.SupportedFrameworks.Intersect(SupportedFrameworks).Any() &&
                         Path.GetExtension(file.Path) == AssemblyExtension &&
                         !string.IsNullOrEmpty(ResolvePathPlatformName(file.Path))
                   select file.Path;
        }

        static IEnumerable<LibraryFolder> GetLibraryFolders(IPackage package, string installPath)
        {
            return from file in package.GetFiles(BuildDirectory)
                   where file.SupportedFrameworks.Contains(NativeFramework)
                   group file by Path.GetDirectoryName(file.Path) into folder
                   let platform = ResolvePathPlatformName(folder.Key)
                   where !string.IsNullOrWhiteSpace(platform)
                   select new LibraryFolder(Path.Combine(installPath, folder.Key), platform);
        }

        static IEnumerable<IPackageAssemblyReference> GetCompatibleAssemblyReferences(IPackage package)
        {
            return from reference in package.AssemblyReferences
                   where !reference.SupportedFrameworks.Any() ||
                         reference.SupportedFrameworks.Intersect(SupportedFrameworks).Any()
                   group reference by reference.Name into referenceGroup
                   from reference in referenceGroup
                       .OrderByDescending(reference => reference.TargetFramework != null
                           ? reference.TargetFramework.FullName
                           : null)
                       .Take(1)
                   select reference;
        }

        void RegisterAssemblyLocations(IPackage package, string installPath, string relativePath, bool addReferences)
        {
            var assemblyLocations = GetAssemblyLocations(package);
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

        void RemoveAssemblyLocations(IPackage package, string installPath, bool removeReference)
        {
            var assemblyLocations = GetAssemblyLocations(package);
            RemoveAssemblyLocations(assemblyLocations, installPath, removeReference);
        }

        void RemoveAssemblyLocations(IEnumerable<string> assemblyLocations, string installPath, bool removeReference)
        {
            foreach (var path in assemblyLocations)
            {
                var assemblyFile = Path.Combine(installPath, path);
                var assemblyName = AssemblyName.GetAssemblyName(assemblyFile);
                packageConfiguration.AssemblyLocations.Remove(Tuple.Create(assemblyName.Name, assemblyName.ProcessorArchitecture));
                if (removeReference)
                {
                    packageConfiguration.AssemblyReferences.Remove(assemblyName.Name);
                }
            }
        }

        void RegisterLibraryFolders(IPackage package, string installPath)
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

        void RemoveLibraryFolders(IPackage package, string installPath)
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
            var contentDirectory = new DirectoryInfo(Path.Combine(installPath, Constants.ContentDirectory, contentPath));
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

        void RemoveContentFolders(IPackage package, string installPath, string contentPath)
        {
            var contentDirectory = new DirectoryInfo(Path.Combine(installPath, Constants.ContentDirectory, contentPath));
            if (contentDirectory.Exists)
            {
                var bootstrapperContent = new DirectoryInfo(Path.Combine(bootstrapperDirectory, contentPath));
                if (bootstrapperContent.Exists)
                {
                    foreach (var file in package.GetFiles(Path.Combine(Constants.ContentDirectory, contentPath)))
                    {
                        var path = file.Path.Split(DirectorySeparators, 2)[1];
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

        void packageManager_PackageInstalling(object sender, PackageOperationEventArgs e)
        {
            var package = e.Package;
            var entryPoint = package.Id + BonsaiExtension;
            var executablePackage = package.GetContentFiles().Any(file => file.EffectivePath == entryPoint);
            if (executablePackage)
            {
                galleryRepository.AddPackage(package);
                e.Cancel = true;
            }
            else
            {
                var installPath = e.InstallPath;
                var pivots = OverlayHelper.FindPivots(package, installPath).ToArray();
                if (pivots.Length > 0)
                {
                    var overlayVersion = OverlayHelper.FindOverlayVersion(package);
                    var overlayManager = OverlayHelper.CreateOverlayManager(packageManager.SourceRepository, installPath);
                    overlayManager.Logger = packageManager.Logger;
                    foreach (var pivot in pivots)
                    {
                        var pivotPackage = overlayManager.SourceRepository.FindPackage(pivot, overlayVersion);
                        if (pivotPackage == null) throw new InvalidOperationException(string.Format("The package '{0}' could not be found.", pivot));
                        overlayManager.InstallPackage(pivotPackage, false, false);
                    }
                }
            }
        }

        void packageManager_PackageInstalled(object sender, PackageOperationEventArgs e)
        {
            var package = e.Package;
            var taggedPackage = IsTaggedPackage(package);
            var installPath = GetRelativePath(e.InstallPath);
            if (!packageConfiguration.Packages.Contains(package.Id))
            {
                packageConfiguration.Packages.Add(package.Id, package.Version.ToString());
            }
            else packageConfiguration.Packages[package.Id].Version = package.Version.ToString();

            AddContentFolders(e.InstallPath, ExtensionsDirectory);
            RegisterLibraryFolders(package, installPath);
            RegisterAssemblyLocations(package, e.InstallPath, installPath, false);
            var pivots = OverlayHelper.FindPivots(package, e.InstallPath).ToArray();
            if (pivots.Length > 0)
            {
                var overlayManager = OverlayHelper.CreateOverlayManager(packageManager.SourceRepository, e.InstallPath);
                foreach (var pivot in pivots)
                {
                    var pivotPackage = overlayManager.LocalRepository.FindPackage(pivot);
                    RegisterLibraryFolders(pivotPackage, installPath);
                    RegisterAssemblyLocations(pivotPackage, e.InstallPath, installPath, false);
                }
            }

            var assemblyLocations = GetCompatibleAssemblyReferences(package).Select(reference => reference.Path);
            RegisterAssemblyLocations(assemblyLocations, e.InstallPath, installPath, taggedPackage);
            packageConfiguration.Save();

            if (package.Id == bootstrapperPackageId && package.Version > bootstrapperVersion)
            {
                var bootstrapperFileName = Path.GetFileName(bootstrapperExePath);
                var bootstrapperFile = package.GetFiles().FirstOrDefault(file => Path.GetFileName(file.Path).Equals(bootstrapperFileName, StringComparison.OrdinalIgnoreCase));
                if (bootstrapperFile == null)
                {
                    throw new InvalidOperationException(Resources.BootstrapperMissingFromPackage);
                }

                var backupExePath = bootstrapperExePath + OldExtension;
                File.Move(bootstrapperExePath, backupExePath);
                UpdateFile(bootstrapperExePath, bootstrapperFile);
            }
        }

        void packageManager_PackageUninstalling(object sender, PackageOperationEventArgs e)
        {
            var package = e.Package;
            var taggedPackage = IsTaggedPackage(package);
            var installPath = GetRelativePath(e.InstallPath);
            packageConfiguration.Packages.Remove(package.Id);

            RemoveContentFolders(package, e.InstallPath, ExtensionsDirectory);
            RemoveLibraryFolders(package, installPath);
            RemoveAssemblyLocations(package, e.InstallPath, false);
            var pivots = OverlayHelper.FindPivots(package, e.InstallPath).ToArray();
            if (pivots.Length > 0)
            {
                var overlayManager = OverlayHelper.CreateOverlayManager(packageManager.SourceRepository, e.InstallPath);
                foreach (var pivot in pivots)
                {
                    var pivotPackage = overlayManager.LocalRepository.FindPackage(pivot);
                    RemoveLibraryFolders(pivotPackage, installPath);
                    RemoveAssemblyLocations(pivotPackage, e.InstallPath, false);
                }
            }

            var assemblyLocations = GetCompatibleAssemblyReferences(package).Select(reference => reference.Path);
            RemoveAssemblyLocations(assemblyLocations, e.InstallPath, taggedPackage);
            packageConfiguration.Save();

            if (pivots.Length > 0)
            {
                var overlayManager = OverlayHelper.CreateOverlayManager(packageManager.SourceRepository, e.InstallPath);
                overlayManager.Logger = packageManager.Logger;
                foreach (var pivot in pivots)
                {
                    overlayManager.UninstallPackage(pivot);
                }
            }
        }

        void UpdateFile(string path, IPackageFile file)
        {
            using (Stream fromStream = file.GetStream(), toStream = File.Create(path))
            {
                fromStream.CopyTo(toStream);
            }
        }

        public void Dispose()
        {
            packageManager.PackageInstalled -= packageManager_PackageInstalled;
            packageManager.PackageUninstalling -= packageManager_PackageUninstalling;
            OptimizedZipPackage.PurgeCache();
        }
    }
}
