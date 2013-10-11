﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Bonsai.Design;
using Bonsai.Expressions;
using Bonsai.Configuration;
using NuGet;
using System.Threading.Tasks;
using Bonsai.NuGet;
using PackageReference = Bonsai.Configuration.PackageReference;

namespace Bonsai.Editor
{
    static class PackageHelper
    {
        internal static void RunPackageOperation(EventLogger logger, Func<Task> operationFactory)
        {
            using (var dialog = new PackageOperationDialog { ShowInTaskbar = true })
            {
                dialog.RegisterEventLogger(logger);
                var operation = operationFactory();
                operation.ContinueWith(task =>
                {
                    if (!task.IsFaulted)
                    {
                        dialog.BeginInvoke((Action)dialog.Close);
                    }
                });
                dialog.ShowDialog();
            }
        }

        static SemanticVersion ParseVersion(string version)
        {
            if (string.IsNullOrEmpty(version)) return null;
            return SemanticVersion.Parse(version);
        }

        internal static Task<IPackage> StartInstallPackage(this IPackageManager packageManager, string packageId, SemanticVersion version)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    packageManager.Logger.Log(MessageLevel.Info, "Checking for latest version of '{0}'.", packageId);
                    var package = packageManager.SourceRepository.FindPackage(packageId);
                    if (package == null) throw new InvalidOperationException(string.Format("The package '{0}' could not be found.", packageId));
                    packageManager.InstallPackage(package, false, true);
                    return package;
                }
                catch (Exception ex)
                {
                    packageManager.Logger.Log(MessageLevel.Error, ex.Message);
                    throw;
                }
            });
        }

        internal static IEnumerable<PackageReference> GetMissingPackages(IEnumerable<PackageReference> packages, IPackageRepository repository)
        {
            return from package in packages
                   let version = ParseVersion(package.Version)
                   where !repository.Exists(package.Id, version)
                   select package;
        }

        internal static IEnumerable<Task<IPackage>> StartRestorePackages(this IPackageManager packageManager, IEnumerable<PackageReference> packages)
        {
            return packages.Select(package => StartRestorePackage(packageManager, package));
        }

        internal static Task<IPackage> StartRestorePackage(this IPackageManager packageManager, PackageReference package)
        {
            return StartRestorePackage(packageManager, package.Id, ParseVersion(package.Version));
        }

        internal static Task<IPackage> StartRestorePackage(this IPackageManager packageManager, string id, SemanticVersion version)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    packageManager.Logger.Log(MessageLevel.Info, "Restoring '{0}' package.", id);
                    var package = packageManager.SourceRepository.FindPackage(id, version);
                    if (package == null)
                    {
                        var errorMessage = string.Format("Unable to find version '{1}' of package '{0}'.", id, version);
                        throw new InvalidOperationException(errorMessage);
                    }
                    return package;
                }
                catch (Exception ex)
                {
                    packageManager.Logger.Log(MessageLevel.Error, ex.Message);
                    throw;
                }
            });
        }
    }
}
