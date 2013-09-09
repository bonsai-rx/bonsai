using Bonsai.Configuration;
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
        IPackageManager packageManager;
        PackageConfiguration packageConfiguration;
        static readonly IEnumerable<FrameworkName> SupportedFrameworks = new[]
        {
            new FrameworkName(".NETFramework,Version=v4.0"),
            new FrameworkName(".NETFramework,Version=v4.0,Profile=Client"),
            new FrameworkName(".NETFramework,Version=v3.5"),
            new FrameworkName(".NETFramework,Version=v3.5,Profile=Client")
        };

        public PackageConfigurationUpdater(PackageConfiguration configuration, IPackageManager manager)
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
            foreach (var path in GetLibraryFolders(package, installPath))
            {
                packageConfiguration.LibraryFolders.Add(path);
            }

            foreach (var reference in GetCompatibleAssemblyReferences(package))
            {
                var referencePath = Path.Combine(installPath, reference.Path);
                var referenceName = Path.GetFileNameWithoutExtension(reference.Name);
                packageConfiguration.AssemblyLocations.Add(referenceName, referencePath);
                if (taggedPackage)
                {
                    packageConfiguration.Packages.Add(referenceName);
                }
            }

            packageConfiguration.Save();
        }

        void packageManager_PackageUninstalling(object sender, PackageOperationEventArgs e)
        {
            var package = e.Package;
            var installPath = e.InstallPath;
            var taggedPackage = IsTaggedPackage(package);
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
                    packageConfiguration.Packages.Remove(referenceName);
                }
            }

            packageConfiguration.Save();
        }

        public void Dispose()
        {
            packageManager.PackageInstalled -= packageManager_PackageInstalled;
            packageManager.PackageUninstalling -= packageManager_PackageUninstalling;
        }
    }
}
