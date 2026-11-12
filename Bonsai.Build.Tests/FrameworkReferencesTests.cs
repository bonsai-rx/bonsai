using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Build.Tests;

[TestClass]
public sealed class FrameworkReferencesTests
{
    static string[] GetFileNames(params string[] assemblyNames)
    {
        return FrameworkReferences
            .GetReferencePaths(assemblyNames)
            .Select(Path.GetFileName)
            .ToArray();
    }

    [TestMethod]
    public void ReferencePathsIncludeCoreLibrary()
    {
        var fileNames = GetFileNames();
#if NETFRAMEWORK
        CollectionAssert.Contains(fileNames, "mscorlib.dll");
#else
        CollectionAssert.Contains(fileNames, "System.Private.CoreLib.dll");
        CollectionAssert.Contains(fileNames, "System.Runtime.dll");
#endif
    }

    [TestMethod]
    public void ReferencePathsIncludeRequestedAssemblyWithOrWithoutExtension()
    {
        CollectionAssert.Contains(GetFileNames("System.Xml"), "System.Xml.dll");
        CollectionAssert.Contains(GetFileNames("System.Xml.dll"), "System.Xml.dll");
    }

    [TestMethod]
    public void ReferencePathsIgnoreUnknownAssembly()
    {
        CollectionAssert.DoesNotContain(GetFileNames("Unknown.Assembly"), "Unknown.Assembly.dll");
    }

    [TestMethod]
    public void ReferencePathsAreUnique()
    {
        var paths = FrameworkReferences.GetReferencePaths(new[] { "System", "System.dll", "SYSTEM" }).ToArray();
        Assert.AreEqual(paths.Length, paths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }
}
