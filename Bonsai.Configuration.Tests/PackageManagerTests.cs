using System.Threading;
using System.Threading.Tasks;
using Bonsai.NuGet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Bonsai.Configuration.Tests;

[TestClass]
public sealed class PackageManagerTests
{
    static readonly NuGetFramework ProjectFramework = NuGetFramework.Parse("net472");
    static readonly PackageIdentity DependencyV1 = new("Dependency", NuGetVersion.Parse("1.0.0"));
    static readonly PackageIdentity DependencyV2 = new("Dependency", NuGetVersion.Parse("2.0.0"));
    static readonly PackageIdentity DependentV1 = new("Dependent", NuGetVersion.Parse("1.0.0"));
    static readonly PackageIdentity DependentV2 = new("Dependent", NuGetVersion.Parse("2.0.0"));

    LocalPackageFeed feed;

    [TestInitialize]
    public void Initialize()
    {
        feed = new LocalPackageFeed();
    }

    [TestCleanup]
    public void Cleanup()
    {
        feed.Dispose();
    }

    static PackageDependency DependsOn(string versionRange)
    {
        return new PackageDependency(DependencyV1.Id, VersionRange.Parse(versionRange));
    }

    PackageManager CreatePackageManager()
    {
        return new PackageManager(feed.Settings, new PackageSourceProvider(feed.Settings), feed.PackagesPath);
    }

    static bool IsInstalled(PackageManager packageManager, PackageIdentity package)
    {
        return packageManager.PathResolver.GetInstalledPath(package) != null;
    }

    [TestMethod]
    // Regression test for https://github.com/bonsai-rx/bonsai/issues/1042
    public async Task InstallPackage_VersionOutsideInstalledDependentRange_ThrowsAndKeepsInstalledVersion()
    {
        feed.AddPackage(DependencyV1);
        feed.AddPackage(DependencyV2);
        feed.AddPackage(DependentV1, DependsOn("[1.0.0, 2.0.0)"));
        var packageManager = CreatePackageManager();
        await packageManager.InstallPackageAsync(DependentV1, ProjectFramework, ignoreDependencies: false, CancellationToken.None);

        var exception = await Assert.ThrowsExceptionAsync<IncompatiblePackageException>(
            () => packageManager.InstallPackageAsync(DependencyV2, ProjectFramework, ignoreDependencies: false, CancellationToken.None));
        StringAssert.Contains(exception.Message, "'Dependent 1.0.0' requires 'Dependency (>= 1.0.0 && < 2.0.0)'");
        Assert.AreEqual(1, exception.DependencyConflicts.Count);
        Assert.AreEqual(DependentV1, exception.DependencyConflicts[0].Package);
        Assert.AreEqual(DependencyV2, exception.DependencyConflicts[0].ResolvedPackage);
        Assert.IsTrue(IsInstalled(packageManager, DependencyV1), "the installed dependency version should be kept");
        Assert.IsFalse(IsInstalled(packageManager, DependencyV2), "the incompatible dependency version should not be installed");
    }

    [TestMethod]
    public async Task InstallPackage_VersionInsideInstalledDependentRange_ReplacesInstalledVersion()
    {
        feed.AddPackage(DependencyV1);
        feed.AddPackage(DependencyV2);
        feed.AddPackage(DependentV1, DependsOn("1.0.0"));
        var packageManager = CreatePackageManager();
        await packageManager.InstallPackageAsync(DependentV1, ProjectFramework, ignoreDependencies: false, CancellationToken.None);

        await packageManager.InstallPackageAsync(DependencyV2, ProjectFramework, ignoreDependencies: false, CancellationToken.None);
        Assert.IsTrue(IsInstalled(packageManager, DependencyV2), "the compatible dependency version should be installed");
        Assert.IsFalse(IsInstalled(packageManager, DependencyV1), "the previous dependency version should be removed");
    }

    [TestMethod]
    public async Task ValidatePackageDependencies_IncompatiblePackageSet_Throws()
    {
        feed.AddPackage(DependencyV1);
        feed.AddPackage(DependencyV2);
        feed.AddPackage(DependentV1, DependsOn("[1.0.0, 2.0.0)"));
        var packageManager = CreatePackageManager();
        await packageManager.InstallPackageAsync(DependentV1, ProjectFramework, ignoreDependencies: false, CancellationToken.None);

        var exception = await Assert.ThrowsExceptionAsync<IncompatiblePackageException>(
            () => packageManager.ValidatePackageDependenciesAsync(new[] { DependentV1, DependencyV2 }, ProjectFramework, CancellationToken.None));
        StringAssert.Contains(exception.Message, "'Dependent 1.0.0' requires 'Dependency (>= 1.0.0 && < 2.0.0)'");
    }

    [TestMethod]
    public async Task GetDependencyConflicts_IncompatiblePackageSet_ReturnsConflictsWithoutThrowing()
    {
        feed.AddPackage(DependencyV2);
        feed.AddPackage(DependentV1, DependsOn("[1.0.0, 2.0.0)"));
        var packageManager = CreatePackageManager();

        var conflicts = await packageManager.GetDependencyConflictsAsync(new[] { DependentV1, DependencyV2 }, ProjectFramework, CancellationToken.None);
        Assert.AreEqual(1, conflicts.Count);
        Assert.AreEqual(DependentV1, conflicts[0].Package);
        Assert.AreEqual(DependencyV1.Id, conflicts[0].Dependency.Id);
        Assert.AreEqual(DependencyV2, conflicts[0].ResolvedPackage);
    }

    [TestMethod]
    public async Task ValidatePackageDependencies_CompatiblePackageSet_InstallsNothing()
    {
        feed.AddPackage(DependencyV1);
        feed.AddPackage(DependentV1, DependsOn("[1.0.0, 2.0.0)"));
        var packageManager = CreatePackageManager();

        await packageManager.ValidatePackageDependenciesAsync(new[] { DependentV1, DependencyV1 }, ProjectFramework, CancellationToken.None);
        Assert.IsFalse(IsInstalled(packageManager, DependentV1), "validation should not install packages");
        Assert.IsFalse(IsInstalled(packageManager, DependencyV1), "validation should not install packages");
    }

    [TestMethod]
    public async Task InstallPackages_DependentUpdatedWithDependency_ReplacesBothInstalledVersions()
    {
        feed.AddPackage(DependencyV1);
        feed.AddPackage(DependencyV2);
        feed.AddPackage(DependentV1, DependsOn("[1.0.0, 2.0.0)"));
        feed.AddPackage(DependentV2, DependsOn("[2.0.0, 3.0.0)"));
        var packageManager = CreatePackageManager();
        await packageManager.InstallPackageAsync(DependentV1, ProjectFramework, ignoreDependencies: false, CancellationToken.None);

        await packageManager.InstallPackagesAsync(new[] { DependencyV2, DependentV2 }, ProjectFramework, CancellationToken.None);
        Assert.IsTrue(IsInstalled(packageManager, DependencyV2), "the dependency should be updated");
        Assert.IsTrue(IsInstalled(packageManager, DependentV2), "the dependent should be updated");
        Assert.IsFalse(IsInstalled(packageManager, DependencyV1), "the previous dependency version should be removed");
        Assert.IsFalse(IsInstalled(packageManager, DependentV1), "the previous dependent version should be removed");
    }
}
