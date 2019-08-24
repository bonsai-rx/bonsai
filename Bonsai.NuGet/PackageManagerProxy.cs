using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    class PackageManagerProxy : IPackageManager
    {
        IPackageManager packageManager;
        IPackageRepository sourceRepository;

        public IPackageManager PackageManager
        {
            get { return packageManager; }
            set
            {
                if (packageManager != null)
                {
                    packageManager.PackageInstalled -= OnPackageInstalled;
                    packageManager.PackageInstalling -= OnPackageInstalling;
                    packageManager.PackageUninstalled -= OnPackageUninstalled;
                    packageManager.PackageUninstalling -= OnPackageUninstalling;
                }

                packageManager = value;
                if (packageManager != null)
                {
                    packageManager.PackageInstalled += OnPackageInstalled;
                    packageManager.PackageInstalling += OnPackageInstalling;
                    packageManager.PackageUninstalled += OnPackageUninstalled;
                    packageManager.PackageUninstalling += OnPackageUninstalling;
                }
            }
        }

        public IFileSystem FileSystem
        {
            get { return packageManager != null ? packageManager.FileSystem : null; }
            set { if (packageManager != null) packageManager.FileSystem = value; }
        }

        public void InstallPackage(string packageId, SemanticVersion version, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
            var manager = packageManager;
            if (manager != null)
            {
                manager.InstallPackage(packageId, version, ignoreDependencies, allowPrereleaseVersions);
            }
        }

        public void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions, bool ignoreWalkInfo)
        {
            var manager = packageManager;
            if (manager != null)
            {
                manager.InstallPackage(package, ignoreDependencies, allowPrereleaseVersions, ignoreWalkInfo);
            }
        }

        public void InstallPackage(IPackage package, bool ignoreDependencies, bool allowPrereleaseVersions)
        {
            var manager = packageManager;
            if (manager != null)
            {
                manager.InstallPackage(package, ignoreDependencies, allowPrereleaseVersions);
            }
        }

        public IPackageRepository LocalRepository
        {
            get { return packageManager != null ? packageManager.LocalRepository : null; }
        }

        public ILogger Logger
        {
            get { return packageManager != null ? packageManager.Logger : null; }
            set { if (packageManager != null) packageManager.Logger = value; }
        }

        public event EventHandler<PackageOperationEventArgs> PackageInstalled;

        public event EventHandler<PackageOperationEventArgs> PackageInstalling;

        public event EventHandler<PackageOperationEventArgs> PackageUninstalled;

        public event EventHandler<PackageOperationEventArgs> PackageUninstalling;

        private void OnPackageInstalled(object sender, PackageOperationEventArgs e)
        {
            var handler = PackageInstalled;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnPackageInstalling(object sender, PackageOperationEventArgs e)
        {
            var handler = PackageInstalling;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnPackageUninstalled(object sender, PackageOperationEventArgs e)
        {
            var handler = PackageUninstalled;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnPackageUninstalling(object sender, PackageOperationEventArgs e)
        {
            var handler = PackageUninstalling;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public IPackagePathResolver PathResolver
        {
            get { return packageManager != null ? packageManager.PathResolver : null; }
        }

        public IPackageRepository SourceRepository
        {
            get { return sourceRepository; }
            set { sourceRepository = value; }
        }

        public void UninstallPackage(string packageId, SemanticVersion version, bool forceRemove, bool removeDependencies)
        {
            var manager = packageManager;
            if (manager != null)
            {
                manager.UninstallPackage(packageId, version, forceRemove, removeDependencies);
            }
        }

        public void UninstallPackage(IPackage package, bool forceRemove, bool removeDependencies)
        {
            var manager = packageManager;
            if (manager != null)
            {
                manager.UninstallPackage(package, forceRemove, removeDependencies);
            }
        }

        public void UpdatePackage(string packageId, IVersionSpec versionSpec, bool updateDependencies, bool allowPrereleaseVersions)
        {
            var manager = packageManager;
            if (manager != null)
            {
                manager.UpdatePackage(packageId, versionSpec, updateDependencies, allowPrereleaseVersions);
            }
        }

        public void UpdatePackage(string packageId, SemanticVersion version, bool updateDependencies, bool allowPrereleaseVersions)
        {
            var manager = packageManager;
            if (manager != null)
            {
                manager.UpdatePackage(packageId, version, updateDependencies, allowPrereleaseVersions);
            }
        }

        public void UpdatePackage(IPackage newPackage, bool updateDependencies, bool allowPrereleaseVersions)
        {
            var manager = packageManager;
            if (manager != null)
            {
                manager.UpdatePackage(newPackage, updateDependencies, allowPrereleaseVersions);
            }
        }

        public DependencyVersion DependencyVersion
        {
            get { return packageManager != null ? packageManager.DependencyVersion : default(DependencyVersion); }
            set
            {
                var manager = packageManager;
                if (manager != null)
                {
                    manager.DependencyVersion = value;
                }
            }
        }

        public bool WhatIf
        {
            get { return packageManager != null ? packageManager.WhatIf : default(bool); }
            set
            {
                var manager = packageManager;
                if (manager != null)
                {
                    manager.WhatIf = value;
                }
            }
        }
    }
}
