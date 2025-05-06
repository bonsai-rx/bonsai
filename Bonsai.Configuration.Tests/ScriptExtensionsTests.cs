using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Configuration.Tests;

[TestClass]
[DoNotParallelize] // Tests involve filesystem at current working directory and must not be ran concurrently
public sealed class ScriptExtensionsTests
{
    private static void EnsureCleanWorkspace()
    {
        if (File.Exists("Extensions.csproj"))
            File.Delete("Extensions.csproj");
    }

    private static readonly PackageReference DummyPackageA = new("DummyPackageA", "3226.42.1337");
    private static readonly PackageReference DummyPackageB = new("DummyPackageB", "126.484.30");
    private static PackageConfiguration CreateDummyPackageConfiguration()
    {
        string dummyBonsaiConfig =
            $"""
            <?xml version="1.0" encoding="utf-8"?>
            <PackageConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
              <Packages>
                <Package id="{DummyPackageA.Id}" version="{DummyPackageA.Version}" />
                <Package id="{DummyPackageB.Id}" version="{DummyPackageB.Version}" />
              </Packages>
              <AssemblyReferences>
              </AssemblyReferences>
              <AssemblyLocations>
                <AssemblyLocation assemblyName="{DummyPackageA.Id}" processorArchitecture="MSIL" location="Packages/{DummyPackageA.Id}.{DummyPackageA.Version}/{DummyPackageA.Id}.dll" />
                <AssemblyLocation assemblyName="{DummyPackageB.Id}" processorArchitecture="MSIL" location="Packages/{DummyPackageB.Id}.{DummyPackageB.Version}/{DummyPackageB.Id}.dll" />
              </AssemblyLocations>
              <LibraryFolders>
              </LibraryFolders>
            </PackageConfiguration>
            """;

        var serializer = new XmlSerializer(typeof(PackageConfiguration));
        using XmlReader reader = XmlReader.Create(new StringReader(dummyBonsaiConfig));
        var configuration = (PackageConfiguration)serializer.Deserialize(reader);
        configuration.ConfigurationFile = Path.GetFullPath("DummyBonsai.config");
        return configuration;
    }

    [TestMethod]
    public void HasDefaultProjectMetadata()
    {
        EnsureCleanWorkspace();

        var packageConfiguration = CreateDummyPackageConfiguration();
        using var extensions = new ScriptExtensions(packageConfiguration, $"{nameof(HasDefaultProjectMetadata)}Output");

        Assert.IsFalse(File.Exists("Extensions.csproj"));
        ScriptExtensionsProjectMetadata metadata = extensions.LoadProjectMetadata();

        Assert.IsFalse(metadata.Exists);
        Assert.AreEqual(default(ScriptExtensionsProjectMetadata).GetProjectXml(), metadata.GetProjectXml());
        Assert.IsFalse(File.Exists("Extensions.csproj")); // Project file should not be written automatically
    }

    [TestMethod]
    public void WillLoadProjectMetadata()
    {
        EnsureCleanWorkspace();

        File.WriteAllText
        (
            "Extensions.csproj",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                <TargetFramework>net472</TargetFramework>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="FakePackage" Version="1.0.0" />
              </ItemGroup>
            </Project>
            """
        );

        var packageConfiguration = CreateDummyPackageConfiguration();
        using var extensions = new ScriptExtensions(packageConfiguration, $"{nameof(WillLoadProjectMetadata)}Output");
        ScriptExtensionsProjectMetadata metadata = extensions.LoadProjectMetadata();
        Assert.IsTrue(metadata.Exists);
        Assert.IsTrue(metadata.AllowUnsafeBlocks);
        CollectionAssert.Contains(metadata.GetPackageReferences().ToArray(), "FakePackage");
    }

    [TestMethod]
    public void UpdateProjectMetadataInitializesProject()
    {
        EnsureCleanWorkspace();

        var packageConfiguration = CreateDummyPackageConfiguration();
        using var extensions = new ScriptExtensions(packageConfiguration, $"{nameof(UpdateProjectMetadataInitializesProject)}Output");

        Assert.IsFalse(File.Exists("Extensions.csproj"));
        extensions.UpdateProjectMetadata(default);

        Assert.IsTrue(File.Exists("Extensions.csproj"));
        Assert.AreEqual(default(ScriptExtensionsProjectMetadata).GetProjectXml(), extensions.LoadProjectMetadata().GetProjectXml());
    }

    [TestMethod]
    public void AddAssemblyReferencesInitializesProject()
    {
        EnsureCleanWorkspace();

        var packageConfiguration = CreateDummyPackageConfiguration();
        using var extensions = new ScriptExtensions(packageConfiguration, $"{nameof(AddAssemblyReferencesInitializesProject)}Output");

        Assert.IsFalse(File.Exists("Extensions.csproj"));
        extensions.AddAssemblyReferences([DummyPackageA.Id]);

        Assert.IsTrue(File.Exists("Extensions.csproj"));
        var metadata = extensions.LoadProjectMetadata();
        CollectionAssert.Contains(metadata.GetPackageReferences().ToArray(), DummyPackageA.Id);
    }

    [TestMethod]
    public void AddAssemblyReferencesUpdatesExistingProject()
    {
        EnsureCleanWorkspace();

        File.WriteAllText
        (
            "Extensions.csproj",
            $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                <TargetFramework>net472</TargetFramework>
              </PropertyGroup>

              <ItemGroup>
                <PackageReference Include="{DummyPackageA.Id}" Version="{DummyPackageA.Version}" />
              </ItemGroup>
            </Project>
            """
        );

        var packageConfiguration = CreateDummyPackageConfiguration();
        using var extensions = new ScriptExtensions(packageConfiguration, $"{nameof(AddAssemblyReferencesUpdatesExistingProject)}Output");

        var before = extensions.LoadProjectMetadata().GetPackageReferences();
        CollectionAssert.Contains(before.ToArray(), DummyPackageA.Id);
        CollectionAssert.DoesNotContain(before.ToArray(), DummyPackageB.Id);

        extensions.AddAssemblyReferences([DummyPackageB.Id]);

        var after = extensions.LoadProjectMetadata().GetPackageReferences();
        CollectionAssert.Contains(after.ToArray(), DummyPackageA.Id);
        CollectionAssert.Contains(after.ToArray(), DummyPackageB.Id);
    }

    [TestMethod]
    // Regression test for https://github.com/bonsai-rx/bonsai/issues/2055
    public void AddAssemblyReferencesUpdatesAreNonDestructive()
    {
        EnsureCleanWorkspace();

        // A few edge cases exist that are not handled due to the way System.Xml.Linq works and therefore intentionally not tested:
        // * XML declaration / processing instructions are not preserved -- It is legacy/meaningless to include these in a csproj
        // * Trailing whitespace after the final closing tag
        // * Mixed newlines or newlines not matching the environment -- They're always normalized.
        string projectFileStart =
            $"""
            <Project Sdk="Microsoft.NET.Sdk">
                {"\t"}<PropertyGroup>
            {"\t\t"}<AllowUnsafeBlocks>true</AllowUnsafeBlocks>
                    <TargetFramework>net472</TargetFramework>
              </PropertyGroup>



            <ItemGroup>
                <!-- This is a comment before the first package reference -->
                <PackageReference Include="{DummyPackageA.Id}" Version="{DummyPackageA.Version}" />

            """.NormalizeNewlines();
        string projectFileEnd =
            $"""
                <!-- This one is last! -->
              </ItemGroup>
            </Project>
            """.NormalizeNewlines();

        File.WriteAllText
        (
            "Extensions.csproj",
            projectFileStart + projectFileEnd,
            Encoding.UTF8
        );

        var packageConfiguration = CreateDummyPackageConfiguration();
        using var extensions = new ScriptExtensions(packageConfiguration, $"{nameof(AddAssemblyReferencesUpdatesExistingProject)}Output");

        extensions.AddAssemblyReferences([DummyPackageB.Id]);

        string after = File.ReadAllText("Extensions.csproj");
        Assert.AreEqual(projectFileStart + $"""    <PackageReference Include="{DummyPackageB.Id}" Version="{DummyPackageB.Version}" />{Environment.NewLine}""" + projectFileEnd, after);
    }
}
