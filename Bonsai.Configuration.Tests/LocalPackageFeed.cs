using System;
using System.IO;
using System.Linq;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;

namespace Bonsai.Configuration.Tests;

sealed class LocalPackageFeed : IDisposable
{
    public LocalPackageFeed()
    {
        RootPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(FeedPath);
        Directory.CreateDirectory(PackagesPath);
        File.WriteAllText(
            Path.Combine(RootPath, EnvironmentSelector.NuGetConfig),
            $"""
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <config>
                <add key="globalPackagesFolder" value="{Path.Combine(RootPath, "global")}" />
              </config>
              <packageSources>
                <clear />
                <add key="test" value="{FeedPath}" />
              </packageSources>
            </configuration>
            """);
        Settings = global::NuGet.Configuration.Settings.LoadSpecificSettings(RootPath, EnvironmentSelector.NuGetConfig);
    }

    public string RootPath { get; }

    public string FeedPath => Path.Combine(RootPath, "feed");

    public string PackagesPath => Path.Combine(RootPath, "packages");

    public ISettings Settings { get; }

    public void AddPackage(PackageIdentity package, params PackageDependency[] dependencies)
    {
        var builder = new PackageBuilder
        {
            Id = package.Id,
            Version = package.Version,
            Description = package.Id
        };
        builder.Authors.Add(nameof(LocalPackageFeed));
        builder.Files.Add(new PhysicalPackageFile(new MemoryStream(new byte[] { 0 })) { TargetPath = "content/placeholder.txt" });
        if (dependencies.Length > 0)
        {
            builder.DependencyGroups.Add(new PackageDependencyGroup(NuGetFramework.AnyFramework, dependencies.ToList()));
        }

        using var stream = File.Create(Path.Combine(FeedPath, $"{package.Id}.{package.Version}.nupkg"));
        builder.Save(stream);
    }

    public void Dispose()
    {
        try { Directory.Delete(RootPath, recursive: true); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
