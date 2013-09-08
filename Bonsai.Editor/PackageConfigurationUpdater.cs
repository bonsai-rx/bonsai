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
        static readonly FrameworkName CompatibleFrameworkName = new FrameworkName(".NETFramework,Version=v4.0");

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
            return package.Tags.Contains(Constants.PackageTagFilter);
        }

        static IEnumerable<string> GetLibraryFolders(IPackage package, string installPath)
        {
            return (from file in package.GetContentFiles()
                    where Path.GetExtension(file.EffectivePath) == ".dll"
                    select Path.GetDirectoryName(Path.Combine(installPath, file.Path)))
                    .Distinct();
        }

        static IEnumerable<IPackageAssemblyReference> GetCompatibleAssemblyReferences(IPackage package, FrameworkName name)
        {
            foreach (var reference in package.AssemblyReferences)
            {
                if (!reference.SupportedFrameworks.Contains(name)) continue;
                yield return reference;
            }
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

            foreach (var reference in GetCompatibleAssemblyReferences(package, CompatibleFrameworkName))
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

            foreach (var reference in GetCompatibleAssemblyReferences(package, CompatibleFrameworkName))
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
