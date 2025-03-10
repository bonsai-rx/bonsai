using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Configuration.Tests;

[TestClass]
public sealed class ScriptExtensionsProjectMetadataTests
{
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void DefaultValueStateIsValid(bool useNullXDocument)
    {
        ScriptExtensionsProjectMetadata metadata = useNullXDocument ? new((XDocument)null) : default;
        Assert.IsFalse(metadata.Exists);
        Assert.IsFalse(metadata.AllowUnsafeBlocks);
        CollectionAssert.DoesNotContain(metadata.GetAssemblyReferences().ToArray(), "System.Windows.Forms.dll"); // Winforms is not enabled by default
        Assert.AreEqual(0, metadata.GetPackageReferences().Count());
        Assert.AreEqual
        (
            """
            <Project Sdk="Microsoft.NET.Sdk">            

              <PropertyGroup>
                <TargetFramework>net472</TargetFramework>
              </PropertyGroup>

            </Project>
            """.NormalizeNewlines(),
            metadata.GetProjectXml()
        );
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void AllowUnsafeBlocks(bool enabled)
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
            <AllowUnsafeBlocks>{enabled}</AllowUnsafeBlocks>
          </PropertyGroup> 
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        Assert.AreEqual(enabled, metadata.AllowUnsafeBlocks);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void EnablingWinformsReferencesWinforms(bool enabled)
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
            <UseWindowsForms>{enabled}</UseWindowsForms>
          </PropertyGroup> 
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);

        if (enabled)
            CollectionAssert.Contains(metadata.GetAssemblyReferences().ToArray(), "System.Windows.Forms.dll");
        else
            CollectionAssert.DoesNotContain(metadata.GetAssemblyReferences().ToArray(), "System.Windows.Forms.dll");
    }

    [TestMethod]
    public void GetPackageReferences()
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
          </PropertyGroup> 
          <ItemGroup>
            <PackageReference Include="FakePackageA" Version="3226.42.1337" />
            <PackageReference Include="FakePackageB" Version="126.484.30" />
          </ItemGroup>
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        var references = metadata.GetPackageReferences();
        CollectionAssert.AreEquivalent((string[])["FakePackageA", "FakePackageB"], references.ToArray());
    }

    [TestMethod]
    public void GetPackageReferencesTwoItemGroups()
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
          </PropertyGroup> 
          <ItemGroup>
            <PackageReference Include="FakePackageA" Version="3226.42.1337" />
          </ItemGroup>
          <ItemGroup>
            <PackageReference Include="FakePackageB" Version="126.484.30" />
          </ItemGroup>
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        var references = metadata.GetPackageReferences();
        CollectionAssert.AreEquivalent((string[])["FakePackageA", "FakePackageB"], references.ToArray());
    }

    [TestMethod]
    public void AddNewPackageReference()
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
          </PropertyGroup> 
          <ItemGroup>
            <PackageReference Include="FakePackageA" Version="3226.42.1337" />
          </ItemGroup>
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        var updatedMetadata = metadata.AddPackageReferences([new PackageReference("FakePackageB", "126.484.30")]);

        // Assert the new metadata has the reference but the immutable original does not
        CollectionAssert.AreEquivalent((string[])["FakePackageA", "FakePackageB"], updatedMetadata.GetPackageReferences().ToArray());
        CollectionAssert.AreEquivalent((string[])["FakePackageA"], metadata.GetPackageReferences().ToArray());

        var xml = updatedMetadata.GetProjectXml();
        var xElement = XElement.Parse(xml);

        var itemGroups = xElement.Descendants("ItemGroup").ToArray();
        Assert.AreEqual(1, itemGroups.Length);

        var packageBs = xElement.XPathSelectElements("/ItemGroup/PackageReference[@Include = 'FakePackageB']").ToArray();
        Assert.AreEqual(1, packageBs.Length);
        Assert.AreEqual(itemGroups[0], packageBs[0].Parent);
        Assert.AreEqual("""<PackageReference Include="FakePackageB" Version="126.484.30" />""", packageBs[0].ToString());
    }

    [TestMethod]
    public void AddPackageReferenceToEmptyItemGroup()
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
          </PropertyGroup> 
          <ItemGroup>
          </ItemGroup>
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        Assert.AreEqual(0, metadata.GetPackageReferences().Count());

        metadata = metadata.AddPackageReferences([new PackageReference("FakePackageA", "3226.42.1337")]);
        var xml = metadata.GetProjectXml();
        var xElement = XElement.Parse(xml);
        var itemGroups = xElement.Descendants("ItemGroup").ToArray();
        var packageReferences = xElement.Descendants("PackageReference").ToArray();

        CollectionAssert.AreEquivalent((string[])["FakePackageA"], metadata.GetPackageReferences().ToArray());
        Assert.AreEqual(1, itemGroups.Length);
        Assert.AreEqual(1, packageReferences.Length);
        Assert.AreEqual(itemGroups[0], packageReferences[0].Parent);
        Assert.AreEqual("""<PackageReference Include="FakePackageA" Version="3226.42.1337" />""", packageReferences[0].ToString());
    }

    [TestMethod]
    public void AddPackageReferenceToItemGroupWithoutPackageReferences()
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
          </PropertyGroup> 
          <ItemGroup>
            <None Include="HelloWorld.txt" />
          </ItemGroup>
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        Assert.AreEqual(0, metadata.GetPackageReferences().Count());

        metadata = metadata.AddPackageReferences([new PackageReference("FakePackageA", "3226.42.1337")]);
        var xml = metadata.GetProjectXml();
        var xElement = XElement.Parse(xml);
        var itemGroups = xElement.Descendants("ItemGroup").ToArray();
        var packageReferences = xElement.Descendants("PackageReference").ToArray();

        CollectionAssert.AreEquivalent((string[])["FakePackageA"], metadata.GetPackageReferences().ToArray());
        Assert.AreEqual(1, itemGroups.Length);
        Assert.AreEqual(1, packageReferences.Length);
        Assert.AreEqual(itemGroups[0], packageReferences[0].Parent);
        Assert.AreEqual("""<PackageReference Include="FakePackageA" Version="3226.42.1337" />""", packageReferences[0].ToString());
    }

    [TestMethod]
    public void AddPackageReferenceToEmptyItemGroupAndProperties()
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
          </PropertyGroup> 
          <ItemGroup Condition="false">
          </ItemGroup>
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        Assert.AreEqual(0, metadata.GetPackageReferences().Count());

        metadata = metadata.AddPackageReferences([new PackageReference("FakePackageA", "3226.42.1337")]);
        var xml = metadata.GetProjectXml();
        var xElement = XElement.Parse(xml);
        var itemGroups = xElement.Descendants("ItemGroup").ToArray();
        var packageReferences = xElement.Descendants("PackageReference").ToArray();

        CollectionAssert.AreEquivalent((string[])["FakePackageA"], metadata.GetPackageReferences().ToArray());
        Assert.AreEqual(1, packageReferences.Length);
        Assert.AreEqual("""<PackageReference Include="FakePackageA" Version="3226.42.1337" />""", packageReferences[0].ToString());

        // A fresh item group should've been added for the reference
        Assert.AreEqual(2, xElement.Descendants("ItemGroup").Count());
        Assert.AreNotEqual(xElement.XPathSelectElement("/ItemGroup[@Condition='false']"), packageReferences[0].Parent);
    }

    [TestMethod]
    public void AddPackageReferenceWithoutItemGroup()
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
          </PropertyGroup> 
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        Assert.AreEqual(0, metadata.GetPackageReferences().Count());

        metadata = metadata.AddPackageReferences([new PackageReference("FakePackageA", "3226.42.1337")]);
        var xml = metadata.GetProjectXml();
        var xElement = XElement.Parse(xml);
        var itemGroups = xElement.Descendants("ItemGroup").ToArray();
        var packageReferences = xElement.Descendants("PackageReference").ToArray();

        CollectionAssert.AreEquivalent((string[])["FakePackageA"], metadata.GetPackageReferences().ToArray());
        Assert.AreEqual(1, itemGroups.Length);
        Assert.AreEqual(1, packageReferences.Length);
        Assert.AreEqual(itemGroups[0], packageReferences[0].Parent);
        Assert.AreEqual("""<PackageReference Include="FakePackageA" Version="3226.42.1337" />""", packageReferences[0].ToString());
    }

    [TestMethod]
    public void AddNewPackageReferenceTwoItemGroups()
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
          </PropertyGroup> 
          <ItemGroup>
            <PackageReference Include="FakePackageA" Version="3226.42.1337" />
          </ItemGroup>
          <ItemGroup>
            <PackageReference Include="FakePackageB" Version="126.484.30" />
          </ItemGroup>
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        metadata = metadata.AddPackageReferences([new PackageReference("FakePackageC", "2016.4.14")]);
        var xml = metadata.GetProjectXml();
        var xElement = XElement.Parse(xml);
        var itemGroups = xElement.Descendants("ItemGroup").ToArray();
        var packageReferences = xElement.Descendants("PackageReference").ToArray();

        CollectionAssert.AreEquivalent((string[])["FakePackageA", "FakePackageB", "FakePackageC"], metadata.GetPackageReferences().ToArray());
        Assert.AreEqual(2, itemGroups.Length);
        Assert.AreEqual(3, packageReferences.Length);

        XElement[] packageCs = xElement.XPathSelectElements("/ItemGroup/PackageReference[@Include='FakePackageC']").ToArray();
        Assert.AreEqual(1, packageCs.Length);
        CollectionAssert.Contains(itemGroups, packageCs[0].Parent);
        Assert.AreEqual("""<PackageReference Include="FakePackageC" Version="2016.4.14" />""", packageCs[0].ToString());
    }

    [TestMethod]
    public void UpgradeExistingPackageReference()
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
          </PropertyGroup> 
          <ItemGroup>
            <PackageReference Include="FakePackageA" Version="3226.42.1337" />
            <PackageReference Include="FakePackageB" Version="126.484.30" />
          </ItemGroup>
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        metadata = metadata.AddPackageReferences([new PackageReference("FakePackageA", "3226.42.1338")]);
        var xml = metadata.GetProjectXml();
        var xElement = XElement.Parse(xml);
        var itemGroups = xElement.Descendants("ItemGroup").ToArray();
        var packageReferences = xElement.Descendants("PackageReference").ToArray();

        CollectionAssert.AreEquivalent((string[])["FakePackageA", "FakePackageB"], metadata.GetPackageReferences().ToArray());

        // Package was udpated
        var packageAs = xElement.XPathSelectElements("/ItemGroup/PackageReference[@Include='FakePackageA']").ToArray();
        Assert.AreEqual(1, packageAs.Length);
        Assert.AreEqual("3226.42.1338", packageAs[0].Attribute("Version").Value);
        Assert.AreEqual(-1, xml.IndexOf("3226.42.1337"));

        // Unrelated package untouched
        var packageBs = xElement.XPathSelectElements("/ItemGroup/PackageReference[@Include='FakePackageB']").ToArray();
        Assert.AreEqual(1, packageBs.Length);
        Assert.AreEqual("126.484.30", packageBs[0].Attribute("Version").Value);

        // Only the two packages are present
        Assert.AreEqual(2, xElement.Descendants("PackageReference").Count());
    }

    [TestMethod]
    public void DowngradeExistingPackageReference()
    {
        var document = XDocument.Parse
        ($"""
        <Project Sdk="Microsoft.NET.Sdk">            
          <PropertyGroup>
            <TargetFramework>net472</TargetFramework>
          </PropertyGroup> 
          <ItemGroup>
            <PackageReference Include="FakePackageA" Version="3226.42.1337" />
            <PackageReference Include="FakePackageB" Version="126.484.30" />
          </ItemGroup>
        </Project>
        """, LoadOptions.PreserveWhitespace);
        var metadata = new ScriptExtensionsProjectMetadata(document);
        metadata = metadata.AddPackageReferences([new PackageReference("FakePackageA", "3226.42.1336")]);
        var xml = metadata.GetProjectXml();
        var xElement = XElement.Parse(xml);
        var itemGroups = xElement.Descendants("ItemGroup").ToArray();
        var packageReferences = xElement.Descendants("PackageReference").ToArray();

        CollectionAssert.AreEquivalent((string[])["FakePackageA", "FakePackageB"], metadata.GetPackageReferences().ToArray());

        // Package was not updated
        //TODO: This is how the implementation is written, but is this behavior correct?
        var packageAs = xElement.XPathSelectElements("/ItemGroup/PackageReference[@Include='FakePackageA']").ToArray();
        Assert.AreEqual(1, packageAs.Length);
        Assert.AreEqual("3226.42.1337", packageAs[0].Attribute("Version").Value);
        Assert.AreEqual(-1, xml.IndexOf("3226.42.1336"));

        // Unrelated package untouched
        var packageBs = xElement.XPathSelectElements("/ItemGroup/PackageReference[@Include='FakePackageB']").ToArray();
        Assert.AreEqual(1, packageBs.Length);
        Assert.AreEqual("126.484.30", packageBs[0].Attribute("Version").Value);

        // Only the two packages are present
        Assert.AreEqual(2, xElement.Descendants("PackageReference").Count());
    }
}
