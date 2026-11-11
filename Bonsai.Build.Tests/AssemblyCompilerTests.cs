using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Build.Tests;

[TestClass]
public sealed class AssemblyCompilerTests
{
    static readonly string[] DefaultReferences = FrameworkReferences
        .GetReferencePaths(new[] { "System", "System.Core", "netstandard" })
        .ToArray();

    string outputDirectory;

    [TestInitialize]
    public void CreateOutputDirectory()
    {
        outputDirectory = Path.Combine(Path.GetTempPath(), nameof(AssemblyCompilerTests) + "." + Guid.NewGuid());
        Directory.CreateDirectory(outputDirectory);
    }

    [TestCleanup]
    public void DeleteOutputDirectory()
    {
        try { Directory.Delete(outputDirectory, true); }
        catch { }
    }

    string GetOutputFileName(string assemblyName) => Path.Combine(outputDirectory, assemblyName + ".dll");

    static SourceFile CreateSource(string text) => new("Script.cs", text);

    [TestMethod]
    public void CompileValidSourceWritesAssembly()
    {
        var compiler = new AssemblyCompiler { AssemblyName = "Valid" };
        var outputFileName = GetOutputFileName("Valid");
        var result = compiler.Compile(
            new[] { CreateSource("public class Valid { public int Value => 42; }") },
            DefaultReferences,
            outputFileName);

        Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.AreEqual("Valid", AssemblyName.GetAssemblyName(outputFileName).Name);
    }

    [TestMethod]
    public void CompileInvalidSourceReportsErrorWithPathAndWritesNothing()
    {
        var compiler = new AssemblyCompiler();
        var outputFileName = GetOutputFileName("Invalid");
        var result = compiler.Compile(
            new[] { new SourceFile("Broken.cs", "public class Broken { int Value => missing; }") },
            DefaultReferences,
            outputFileName);

        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.Diagnostics.Any(diagnostic => diagnostic.Contains("Broken.cs") && diagnostic.Contains("error")));
        Assert.IsFalse(File.Exists(outputFileName));
    }

    [TestMethod]
    public void CompileRecentLanguageFeatures()
    {
        // Extension scripts should accept recent C# constructs available on every target framework,
        // here file-scoped namespaces, raw string literals and collection expressions.
        var compiler = new AssemblyCompiler();
        var result = compiler.Compile(
            new[]
            {
                CreateSource(""""
                namespace Recent;

                public static class Features
                {
                    public static string Text => """raw "text" literal""";

                    public static int[] Values => [1, 2, 3];
                }
                """")
            },
            DefaultReferences,
            GetOutputFileName("Recent"));

        Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IncludeDebugInformationWritesSymbols(bool includeDebugInformation)
    {
        var compiler = new AssemblyCompiler { IncludeDebugInformation = includeDebugInformation };
        var outputFileName = GetOutputFileName("Symbols");
        var result = compiler.Compile(new[] { CreateSource("public class Symbols { }") }, DefaultReferences, outputFileName);

        Assert.IsTrue(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        Assert.AreEqual(includeDebugInformation, File.Exists(Path.ChangeExtension(outputFileName, ".pdb")));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void UnsafeCodeRequiresAllowUnsafeBlocks(bool allowUnsafeBlocks)
    {
        var compiler = new AssemblyCompiler { AllowUnsafeBlocks = allowUnsafeBlocks };
        var result = compiler.Compile(
            new[] { CreateSource("public static class Pointers { public static unsafe int Read(int* value) => *value; }") },
            DefaultReferences,
            GetOutputFileName("Unsafe"));

        Assert.AreEqual(allowUnsafeBlocks, result.Success, string.Join(Environment.NewLine, result.Diagnostics));
    }
}
