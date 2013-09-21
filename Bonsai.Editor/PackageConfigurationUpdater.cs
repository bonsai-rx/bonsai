using Bonsai.Configuration;
using Bonsai.Editor.Properties;
using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;

namespace Bonsai.Editor
{
    class PackageConfigurationUpdater : IDisposable
    {
        readonly string bootstrapperExePath;
        readonly string bootstrapperPackageId;
        readonly IPackageManager packageManager;
        readonly PackageConfiguration packageConfiguration;
        static readonly IEnumerable<FrameworkName> SupportedFrameworks = new[]
        {
            new FrameworkName(".NETFramework,Version=v4.0"),
            new FrameworkName(".NETFramework,Version=v4.0,Profile=Client"),
            new FrameworkName(".NETFramework,Version=v3.5"),
            new FrameworkName(".NETFramework,Version=v3.5,Profile=Client")
        };

        public PackageConfigurationUpdater(PackageConfiguration configuration, IPackageManager manager, string bootstrapperPath = null, string bootstrapperId = null)
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
            bootstrapperPackageId = bootstrapperId ?? string.Empty;
            packageManager.PackageInstalled += packageManager_PackageInstalled;
            packageManager.PackageUninstalling += packageManager_PackageUninstalling;
        }

        static bool IsTaggedPackage(IPackage package)
        {
            return package.Tags != null && package.Tags.Contains(Constants.PackageTagFilter);
        }

        static string GetLibraryFolderPlatform(string path)
        {
            var components = path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (components.Length > 3 && components[2] == "bin")
            {
                return components[3];
            }

            return string.Empty;
        }

        static IEnumerable<LibraryFolder> GetLibraryFolders(IPackage package, string installPath)
        {
            return from file in package.GetFiles()
                   where file.Path.StartsWith("build") && file.SupportedFrameworks.Intersect(SupportedFrameworks).Any()
                   group file by Path.GetDirectoryName(file.Path) into folder
                   let platform = GetLibraryFolderPlatform(folder.Key)
                   where !string.IsNullOrWhiteSpace(platform)
                   select new LibraryFolder(Path.Combine(installPath, folder.Key), platform);
        }

        static IEnumerable<IPackageAssemblyReference> GetCompatibleAssemblyReferences(IPackage package)
        {
            return from reference in package.AssemblyReferences
                   where reference.SupportedFrameworks.Intersect(SupportedFrameworks).Any()
                   group reference by reference.Name into referenceGroup
                   from reference in referenceGroup.OrderByDescending(reference => reference.TargetFramework.FullName).Take(1)
                   select reference;
        }

        void packageManager_PackageInstalled(object sender, PackageOperationEventArgs e)
        {
            var package = e.Package;
            var installPath = e.InstallPath;
            var taggedPackage = IsTaggedPackage(package);
            if (!packageConfiguration.Packages.Contains(package.Id))
            {
                packageConfiguration.Packages.Add(package.Id, package.Version.ToString());
            }
            else packageConfiguration.Packages[package.Id].Version = package.Version.ToString();

            foreach (var folder in GetLibraryFolders(package, installPath))
            {
                if (!packageConfiguration.LibraryFolders.Contains(folder.Path))
                {
                    packageConfiguration.LibraryFolders.Add(folder);
                }
                else if(packageConfiguration.LibraryFolders[folder.Path].Platform != folder.Platform)
                {
                    throw new InvalidOperationException(string.Format(Resources.LibraryFolderPlatformMismatchException, folder.Path));
                }
            }

            foreach (var reference in GetCompatibleAssemblyReferences(package))
            {
                var referencePath = Path.Combine(installPath, reference.Path);
                var referenceName = Path.GetFileNameWithoutExtension(reference.Name);
                if (!packageConfiguration.AssemblyLocations.Contains(referenceName))
                {
                    packageConfiguration.AssemblyLocations.Add(referenceName, referencePath);
                }
                else if (packageConfiguration.AssemblyLocations[referenceName].Location != referencePath)
                {
                    throw new InvalidOperationException(string.Format(Resources.AssemblyReferenceLocationMismatchException, referenceName));
                }

                if (taggedPackage && !packageConfiguration.AssemblyReferences.Contains(referenceName))
                {
                    packageConfiguration.AssemblyReferences.Add(referenceName);
                }
            }

            packageConfiguration.Save();

            if (package.Id == bootstrapperPackageId)
            {
                var bootstrapperFileName = Path.GetFileName(bootstrapperExePath);
                var bootstrapperFile = package.GetFiles().FirstOrDefault(file => Path.GetFileName(file.Path).Equals(bootstrapperFileName, StringComparison.OrdinalIgnoreCase));
                if (bootstrapperFile == null)
                {
                    throw new InvalidOperationException(Resources.BootstrapperMissingFromPackage);
                }

                string backupExePath = bootstrapperExePath + ".old";
                MoveFile(bootstrapperExePath, backupExePath);
                UpdateFile(bootstrapperExePath, bootstrapperFile);
            }
        }

        void packageManager_PackageUninstalling(object sender, PackageOperationEventArgs e)
        {
            var package = e.Package;
            var installPath = e.InstallPath;
            var taggedPackage = IsTaggedPackage(package);
            packageConfiguration.Packages.Remove(package.Id);
            foreach (var path in GetLibraryFolders(package, installPath))
            {
                packageConfiguration.LibraryFolders.Remove(path);
            }

            foreach (var reference in GetCompatibleAssemblyReferences(package))
            {
                var referenceName = Path.GetFileNameWithoutExtension(reference.Name);
                packageConfiguration.AssemblyLocations.Remove(referenceName);
                if (taggedPackage)
                {
                    packageConfiguration.AssemblyReferences.Remove(referenceName);
                }
            }

            packageConfiguration.Save();
        }

        void UpdateFile(string path, IPackageFile file)
        {
            using (Stream fromStream = file.GetStream(), toStream = File.Create(path))
            {
                fromStream.CopyTo(toStream);
            }
        }

        void MoveFile(string sourceFileName, string destinationFileName)
        {
            try
            {
                if (File.Exists(destinationFileName))
                {
                    File.Delete(destinationFileName);
                }
            }
            catch (FileNotFoundException)
            {
            }

            File.Move(sourceFileName, destinationFileName);
        }

        public void Dispose()
        {
            packageManager.PackageInstalled -= packageManager_PackageInstalled;
            packageManager.PackageUninstalling -= packageManager_PackageUninstalling;
        }
    }
}
