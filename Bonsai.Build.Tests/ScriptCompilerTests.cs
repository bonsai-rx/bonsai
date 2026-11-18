using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonsai.Build.Tests;

[TestClass]
public sealed class ScriptCompilerTests
{
    static readonly string[] DefaultReferences =
    [
        typeof(object).Assembly.Location,
        Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "System.Runtime.dll")
    ];

    string outputDirectory;

    [TestInitialize]
    public void CreateOutputDirectory()
    {
        outputDirectory = Path.Combine(Path.GetTempPath(), nameof(ScriptCompilerTests) + "." + Guid.NewGuid());
        Directory.CreateDirectory(outputDirectory);
    }

    [TestCleanup]
    public void DeleteOutputDirectory()
    {
        try { Directory.Delete(outputDirectory, true); }
        catch { }
    }

    string GetOutputFileName(string assemblyName) => Path.Combine(outputDirectory, assemblyName + ".dll");

    string CreateSource(string text, string fileName = "Script.cs")
    {
        var path = Path.Combine(outputDirectory, fileName);
        File.WriteAllText(path, text);
        return path;
    }

    static string[] GetArguments(string outputFileName, IEnumerable<string> sourceFiles, params string[] options)
    {
        return ["/nostdlib+", "/nologo", "/target:library", $"/out:{outputFileName}",
                .. DefaultReferences.Select(reference => $"/reference:{reference}"),
                .. options,
                .. sourceFiles];
    }

    int Compile(string[] args, out string output)
    {
        var writer = new StringWriter();
        var exitCode = ScriptCompiler.Run(args, outputDirectory, writer);
        output = writer.ToString();
        return exitCode;
    }

    [TestMethod]
    public void CompileValidSourceWritesAssembly()
    {
        var outputFileName = GetOutputFileName("Valid");
        var args = GetArguments(outputFileName, [CreateSource("public class Valid { public int Value => 42; }")]);

        Assert.AreEqual(ScriptCompiler.SuccessExitCode, Compile(args, out var output), output);
        Assert.AreEqual("Valid", AssemblyName.GetAssemblyName(outputFileName).Name);
    }

    [TestMethod]
    public void CompileInvalidSourceReportsErrorWithPathAndWritesNothing()
    {
        var outputFileName = GetOutputFileName("Invalid");
        var args = GetArguments(outputFileName, [CreateSource("public class Broken { int Value => missing; }", "Broken.cs")]);

        Assert.AreEqual(ScriptCompiler.ErrorExitCode, Compile(args, out var output));
        StringAssert.Contains(output, "Broken.cs");
        StringAssert.Contains(output, "error");
        Assert.IsFalse(File.Exists(outputFileName));
    }

    [TestMethod]
    public void CompileFromResponseFile()
    {
        var outputFileName = GetOutputFileName("Response");
        var responseFile = Path.Combine(outputDirectory, "Response.rsp");
        var args = GetArguments(outputFileName, [CreateSource("public class Response { }")]);
        File.WriteAllLines(responseFile, args.Select(arg => $"\"{arg}\""));

        Assert.AreEqual(ScriptCompiler.SuccessExitCode, Compile([$"@{responseFile}"], out var output), output);
        Assert.IsTrue(File.Exists(outputFileName));
    }

    [TestMethod]
    [DataRow("public class Valid { }", ScriptCompiler.SuccessExitCode)]
    [DataRow("public class Broken { int Value => missing; }", ScriptCompiler.ErrorExitCode)]
    public void PortableCompilerProcessReportsExitCodeAndDiagnostics(string source, int expectedExitCode)
    {
        var outputFileName = GetOutputFileName("Process");
        var responseFile = Path.Combine(outputDirectory, "Process.rsp");
        var args = GetArguments(outputFileName, [CreateSource(source, "Process.cs")]);
        File.WriteAllLines(responseFile, args.Select(arg => $"\"{arg}\""));

        var compilerPath = Path.Combine(AppContext.BaseDirectory, "Bonsai.Build.dll");
        var startInfo = new ProcessStartInfo("dotnet", $"\"{compilerPath}\" \"@{responseFile}\"")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        using var process = Process.Start(startInfo);
        var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
        process.WaitForExit();

        Assert.AreEqual(expectedExitCode, process.ExitCode, output);
        Assert.AreEqual(expectedExitCode == ScriptCompiler.SuccessExitCode, File.Exists(outputFileName));
        if (expectedExitCode == ScriptCompiler.ErrorExitCode)
            StringAssert.Contains(output, "Process.cs");
    }

    [TestMethod]
    [DataRow("7.3", false)]
    [DataRow("12.0", true)]
    [DataRow(null, true)]
    public void LanguageVersionLimitsAvailableSyntax(string languageVersion, bool expectedSuccess)
    {
        var options = languageVersion is null ? Array.Empty<string>() : [$"/langversion:{languageVersion}"];
        var args = GetArguments(
            GetOutputFileName("Versioned"),
            [CreateSource("public static class Values { public static int[] All => [1, 2, 3]; }")],
            options);

        var exitCode = Compile(args, out var output);
        Assert.AreEqual(expectedSuccess, exitCode == ScriptCompiler.SuccessExitCode, output);
    }

    [TestMethod]
    public void InvalidLanguageVersionReportsError()
    {
        var outputFileName = GetOutputFileName("Invalid");
        var args = GetArguments(outputFileName, [CreateSource("public class Valid { }")], "/langversion:not-a-version");

        Assert.AreEqual(ScriptCompiler.ErrorExitCode, Compile(args, out var output));
        StringAssert.Contains(output, "not-a-version");
        Assert.IsFalse(File.Exists(outputFileName));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void IncludeDebugInformationWritesSymbols(bool includeDebugInformation)
    {
        var outputFileName = GetOutputFileName("Symbols");
        var options = includeDebugInformation ? ["/debug:portable"] : Array.Empty<string>();
        var args = GetArguments(outputFileName, [CreateSource("public class Symbols { }")], options);

        Assert.AreEqual(ScriptCompiler.SuccessExitCode, Compile(args, out var output), output);
        Assert.AreEqual(includeDebugInformation, File.Exists(Path.ChangeExtension(outputFileName, ".pdb")));
    }

    [TestMethod]
    public void EmbeddedDebugInformationWritesNoSymbolsFile()
    {
        var outputFileName = GetOutputFileName("Embedded");
        var args = GetArguments(outputFileName, [CreateSource("public class Embedded { }")], "/debug:embedded");

        Assert.AreEqual(ScriptCompiler.SuccessExitCode, Compile(args, out var output), output);
        Assert.IsFalse(File.Exists(Path.ChangeExtension(outputFileName, ".pdb")));
        using var peReader = new PEReader(File.OpenRead(outputFileName));
        Assert.IsTrue(peReader.ReadDebugDirectory().Any(entry => entry.Type == DebugDirectoryEntryType.EmbeddedPortablePdb));
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void DebugSymbolsChecksumMatchesSourceFile(bool byteOrderMark)
    {
        var text = Encoding.UTF8.GetBytes("public class Checksum { }\r\n");
        var contents = byteOrderMark ? Encoding.UTF8.GetPreamble().Concat(text).ToArray() : text;
        var sourcePath = Path.Combine(outputDirectory, "Checksum.cs");
        File.WriteAllBytes(sourcePath, contents);

        var outputFileName = GetOutputFileName("Checksum");
        var args = GetArguments(outputFileName, [sourcePath], "/debug:embedded", "/checksumalgorithm:sha256");
        Assert.AreEqual(ScriptCompiler.SuccessExitCode, Compile(args, out var output), output);

        using var peReader = new PEReader(File.OpenRead(outputFileName));
        var embeddedSymbols = peReader.ReadDebugDirectory().Single(entry => entry.Type == DebugDirectoryEntryType.EmbeddedPortablePdb);
        using var symbolsProvider = peReader.ReadEmbeddedPortablePdbDebugDirectoryData(embeddedSymbols);
        var symbolsReader = symbolsProvider.GetMetadataReader();
        var document = symbolsReader.GetDocument(symbolsReader.Documents.Single());
        CollectionAssert.AreEqual(SHA256.HashData(contents), symbolsReader.GetBlobBytes(document.Hash));
    }

    [TestMethod]
    public void MissingReferenceReportsWarning()
    {
        var missingReference = Path.Combine(outputDirectory, "Missing.dll");
        var args = GetArguments(GetOutputFileName("Valid"), [CreateSource("public class Valid { }")], $"/reference:{missingReference}");

        Assert.AreEqual(ScriptCompiler.SuccessExitCode, Compile(args, out var output), output);
        StringAssert.Contains(output, $"warning: Metadata file '{missingReference}' could not be found.");
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void UnsafeCodeRequiresAllowUnsafeBlocks(bool allowUnsafeBlocks)
    {
        var options = allowUnsafeBlocks ? ["/unsafe+"] : Array.Empty<string>();
        var args = GetArguments(
            GetOutputFileName("Unsafe"),
            [CreateSource("public static class Pointers { public static unsafe int Read(int* value) => *value; }")],
            options);

        var exitCode = Compile(args, out var output);
        Assert.AreEqual(allowUnsafeBlocks, exitCode == ScriptCompiler.SuccessExitCode, output);
    }
}
