using System;
using System.IO;
using System.Linq;
using Bonsai.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Tests;

// Stand-in type used only as the generic type argument in the test workflow, so its assembly
// can be mapped to a package and asserted as a dependency.
public sealed class GenericArgumentMarker
{
}

[TestClass]
public sealed class DependencyInspectorTests
{
    const string ArgumentAssembly = "Bonsai.Tests";
    const string ArgumentPackageId = "Fake.ArgumentTypePackage";
    const string ArgumentPackageVersion = "4.5.6";

    // The workflow declares the closed generic WorkflowProperty<GenericArgumentMarker>. The open
    // type lives in Bonsai.Core, while the generic argument GenericArgumentMarker lives in this test
    // assembly, so the argument assembly is reachable only through the generic type argument.
    const string WorkflowMarkup =
        """
        <?xml version="1.0" encoding="utf-8"?>
        <WorkflowBuilder Version="2.8.5"
                         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                         xmlns:test="clr-namespace:Bonsai.Tests;assembly=Bonsai.Tests"
                         xmlns="https://bonsai-rx.org/2018/workflow">
          <Workflow>
            <Nodes>
              <Expression xsi:type="WorkflowProperty" TypeArguments="test:GenericArgumentMarker">
                <Value xsi:nil="true" />
              </Expression>
            </Nodes>
            <Edges />
          </Workflow>
        </WorkflowBuilder>
        """;

    // A configuration file is loaded from disk so that ConfigurationFile is populated through the
    // public Load entry point. The single assembly location maps the test assembly to a fake package
    // following the default Packages\<id>.<version> layout that GetAssemblyPackageReference parses.
    static readonly string ConfigurationMarkup =
        $"""
        <?xml version="1.0" encoding="utf-8"?>
        <PackageConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
          <Packages>
            <Package id="{ArgumentPackageId}" version="{ArgumentPackageVersion}" />
          </Packages>
          <AssemblyReferences>
            <AssemblyReference assemblyName="{ArgumentAssembly}" />
          </AssemblyReferences>
          <AssemblyLocations>
            <AssemblyLocation assemblyName="{ArgumentAssembly}" processorArchitecture="MSIL" location="Packages\{ArgumentPackageId}.{ArgumentPackageVersion}\lib\{ArgumentAssembly}.dll" />
          </AssemblyLocations>
          <LibraryFolders>
          </LibraryFolders>
        </PackageConfiguration>
        """;

    [TestMethod]
    // Regression test for https://github.com/bonsai-rx/bonsai/issues/2338
    public void GetPackageDependencies_GenericArgumentInDistinctAssembly_IncludesArgumentPackage()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"Bonsai.Tests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var workflowPath = Path.Combine(tempDirectory, "GenericArgumentDependency.bonsai");
            File.WriteAllText(workflowPath, WorkflowMarkup);

            var configurationPath = Path.Combine(tempDirectory, "Bonsai.config");
            File.WriteAllText(configurationPath, ConfigurationMarkup);
            var configuration = ConfigurationHelper.Load(configurationPath);

            var dependencies = DependencyInspector.GetPackageDependencies(new[] { workflowPath }, configuration);
            var packageIds = dependencies.Select(package => package.Id).ToArray();

            CollectionAssert.Contains(packageIds, ArgumentPackageId, "the generic argument package should be collected as a dependency");
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }
}
