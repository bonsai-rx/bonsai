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

    const string CoreAssembly = "Bonsai.Core";
    const string CorePackageId = "Bonsai.Core";
    const string CorePackageVersion = "1.0.0";

    // The workflow contains a single IncludeWorkflow element whose Path references the Bonsai.Core
    // assembly. The inspector loads that assembly through the MetadataLoadContext, while Bonsai.Core
    // is also auto-seeded as the runtime instance of typeof(WorkflowBuilder).Assembly, so the same
    // logical assembly arrives twice as two distinct Assembly instances. The resource name does not
    // exist, which is fine because ReadMetadata swallows the missing-resource failure and the
    // inspector only needs the assembly name from the Path attribute.
    const string IncludeWorkflowMarkup =
        """
        <?xml version="1.0" encoding="utf-8"?>
        <WorkflowBuilder Version="2.8.5"
                         xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                         xmlns="https://bonsai-rx.org/2018/workflow">
          <Workflow>
            <Nodes>
              <Expression xsi:type="IncludeWorkflow" Path="Bonsai.Core:DoesNotExist" />
            </Nodes>
            <Edges />
          </Workflow>
        </WorkflowBuilder>
        """;

    // The assembly location uses the default Packages\<id>.<version> layout so the package mapping
    // resolves Bonsai.Core to its package id, using the realistic Bonsai.Core package id. The test
    // also materializes the real Bonsai.Core.dll at this location so the MetadataLoadContext can load
    // it when resolving the IncludeWorkflow reference, producing a metadata instance distinct from the
    // auto-seeded runtime instance.
    static readonly string CorePackageLocation = $@"Packages\{CorePackageId}.{CorePackageVersion}\lib\{CoreAssembly}.dll";

    static readonly string CoreConfigurationMarkup =
        $"""
        <?xml version="1.0" encoding="utf-8"?>
        <PackageConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
          <Packages>
            <Package id="{CorePackageId}" version="{CorePackageVersion}" />
          </Packages>
          <AssemblyReferences>
            <AssemblyReference assemblyName="{CoreAssembly}" />
          </AssemblyReferences>
          <AssemblyLocations>
            <AssemblyLocation assemblyName="{CoreAssembly}" processorArchitecture="MSIL" location="{CorePackageLocation}" />
          </AssemblyLocations>
          <LibraryFolders>
          </LibraryFolders>
        </PackageConfiguration>
        """;

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

    // Writes the workflow and configuration markup to a fresh temporary directory, loads the
    // configuration through the public Load entry point, and returns the collected package ids. The
    // optional assemblyLocation materializes a real assembly file at that relative location so the
    // MetadataLoadContext resolver can load it from disk.
    static string[] GetWorkflowPackageIds(string workflowFileName, string workflowMarkup, string configurationMarkup, string assemblyLocation = null, string assemblySource = null)
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), $"Bonsai.Tests.{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var workflowPath = Path.Combine(tempDirectory, workflowFileName);
            File.WriteAllText(workflowPath, workflowMarkup);

            var configurationPath = Path.Combine(tempDirectory, "Bonsai.config");
            File.WriteAllText(configurationPath, configurationMarkup);

            if (assemblyLocation != null)
            {
                var targetPath = Path.Combine(tempDirectory, assemblyLocation);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                File.Copy(assemblySource, targetPath);
            }

            var configuration = ConfigurationHelper.Load(configurationPath);

            var dependencies = DependencyInspector.GetPackageDependencies(new[] { workflowPath }, configuration);
            return dependencies.Select(package => package.Id).ToArray();
        }
        finally
        {
            Directory.Delete(tempDirectory, recursive: true);
        }
    }

    [TestMethod]
    // Regression test for https://github.com/bonsai-rx/bonsai/issues/2338
    public void GetPackageDependencies_GenericArgumentInDistinctAssembly_IncludesArgumentPackage()
    {
        var packageIds = GetWorkflowPackageIds("GenericArgumentDependency.bonsai", WorkflowMarkup, ConfigurationMarkup);
        CollectionAssert.Contains(packageIds, ArgumentPackageId, "the generic argument package should be collected as a dependency");
    }

    [TestMethod]
    // Regression test for https://github.com/bonsai-rx/bonsai/issues/2336
    public void GetPackageDependencies_AssemblyArrivesFromRuntimeAndMetadataContext_ReturnsPackageOnce()
    {
        var packageIds = GetWorkflowPackageIds("IncludeWorkflowDependency.bonsai", IncludeWorkflowMarkup, CoreConfigurationMarkup, CorePackageLocation, typeof(WorkflowBuilder).Assembly.Location);
        var distinctCount = packageIds.Distinct(StringComparer.OrdinalIgnoreCase).Count();

        Assert.AreEqual(distinctCount, packageIds.Length, "the collected package ids should contain no duplicates");
        Assert.AreEqual(1, packageIds.Count(id => string.Equals(id, CorePackageId, StringComparison.OrdinalIgnoreCase)), "the core package should appear exactly once");
    }
}
