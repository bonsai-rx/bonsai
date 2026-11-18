using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.NuGet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace Bonsai.Configuration.Tests;

[TestClass]
public sealed class BootstrapperTests
{
    static readonly NuGetFramework ProjectFramework = NuGetFramework.Parse("net472");
    static readonly PackageIdentity Editor = new("Editor", NuGetVersion.Parse("1.0.0"));
    static readonly PackageIdentity DependencyV2 = new("Dependency", NuGetVersion.Parse("2.0.0"));
    static readonly PackageIdentity DependentV1 = new("Dependent", NuGetVersion.Parse("1.0.0"));

    LocalPackageFeed feed;

    [TestInitialize]
    public void Initialize()
    {
        feed = new LocalPackageFeed();
        feed.AddPackage(Editor);
        feed.AddPackage(DependencyV2);
        feed.AddPackage(DependentV1, new PackageDependency(DependencyV2.Id, VersionRange.Parse("[1.0.0, 2.0.0)")));
    }

    [TestCleanup]
    public void Cleanup()
    {
        feed.Dispose();
    }

    string BootstrapperPath => Path.Combine(feed.RootPath, Editor.Id);

    PackageConfiguration CreateIncompatibleConfiguration()
    {
        var fileName = Path.Combine(feed.RootPath, EnvironmentSelector.BonsaiConfig);
        var packages = string.Concat(
            new[] { Editor, DependentV1, DependencyV2 }.Select(package =>
                $"""<Package id="{package.Id}" version="{package.Version}" />"""));
        File.WriteAllText(
            fileName,
            $"""
            <?xml version="1.0" encoding="utf-8"?>
            <PackageConfiguration>
              <Packages>{packages}</Packages>
              <AssemblyReferences>
                <AssemblyReference assemblyName="{Editor.Id}" />
              </AssemblyReferences>
            </PackageConfiguration>
            """);
        return ConfigurationHelper.Load(fileName);
    }

    Bootstrapper CreateBootstrapper()
    {
        return new Bootstrapper(feed.PackagesPath, feed.Settings);
    }

    static bool IsInstalled(Bootstrapper bootstrapper, PackageIdentity package)
    {
        return bootstrapper.PackageManager.PathResolver.GetInstalledPath(package) != null;
    }

    [TestMethod]
    public async Task RunAsync_IncompatiblePackages_ThrowsAndRestoresNothing()
    {
        var configuration = CreateIncompatibleConfiguration();
        var bootstrapper = CreateBootstrapper();

        var exception = await Assert.ThrowsExceptionAsync<IncompatiblePackageException>(
            () => bootstrapper.RunAsync(ProjectFramework, configuration, BootstrapperPath, Editor));
        Assert.AreEqual(DependentV1, exception.DependencyConflicts.Single().Package);
        Assert.AreEqual(DependencyV2, exception.DependencyConflicts.Single().ResolvedPackage);
        Assert.IsFalse(Directory.EnumerateFileSystemEntries(feed.PackagesPath).Any(), "no package should be restored");
    }

    [TestMethod]
    public async Task RunAsync_AllowIncompatiblePackages_RestoresConfigurationVersions()
    {
        var configuration = CreateIncompatibleConfiguration();
        var bootstrapper = CreateBootstrapper();
        bootstrapper.AllowIncompatiblePackages = true;

        await bootstrapper.RunAsync(ProjectFramework, configuration, BootstrapperPath, Editor);
        Assert.IsTrue(IsInstalled(bootstrapper, DependentV1), "the configured dependent version should be restored");
        Assert.IsTrue(IsInstalled(bootstrapper, DependencyV2), "the configured dependency version should be restored");
        await Assert.ThrowsExceptionAsync<IncompatiblePackageException>(
            () => bootstrapper.ValidatePackageConfigurationAsync(ProjectFramework, configuration));
    }
}
