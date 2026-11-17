using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace Bonsai.Build
{
    /// <summary>
    /// Provides functionality for compiling C# source files into a library assembly.
    /// </summary>
    public class AssemblyCompiler
    {
        const string SymbolsExtension = ".pdb";

        /// <summary>
        /// Gets or sets the name of the compiled assembly. If no name is specified, the file name
        /// of the output assembly is used.
        /// </summary>
        public string AssemblyName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether to compile without optimizations and write
        /// debug symbols next to the output assembly.
        /// </summary>
        public bool IncludeDebugInformation { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether to allow code that uses the
        /// <see langword="unsafe"/> keyword.
        /// </summary>
        public bool AllowUnsafeBlocks { get; set; }

        /// <summary>
        /// Gets or sets the C# language version used to parse the source files. If no version is
        /// specified, the latest version supported by the compiler is used.
        /// </summary>
        public string LanguageVersion { get; set; }

        /// <summary>
        /// Compiles the specified source files into a library assembly.
        /// </summary>
        /// <param name="sourceFiles">
        /// The paths of the source files to compile. The checksum written to debug symbols is
        /// computed from the contents of each file.
        /// </param>
        /// <param name="references">
        /// The paths of the assemblies referenced by the source files. Paths that do not exist are
        /// skipped and reported as warnings.
        /// </param>
        /// <param name="outputFileName">The path of the assembly file to write.</param>
        /// <returns>
        /// A <see cref="CompilerResult"/> describing whether the compilation succeeded, and the
        /// errors and warnings reported by the compiler. The output files are written only if the
        /// compilation succeeds.
        /// </returns>
        public CompilerResult Compile(
            IEnumerable<string> sourceFiles,
            IEnumerable<string> references,
            string outputFileName)
        {
            if (sourceFiles is null)
                throw new ArgumentNullException(nameof(sourceFiles));
            if (references is null)
                throw new ArgumentNullException(nameof(references));
            if (string.IsNullOrEmpty(outputFileName))
                throw new ArgumentException("An output file name must be specified.", nameof(outputFileName));

            var languageVersion = Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest;
            if (!string.IsNullOrEmpty(LanguageVersion) &&
                !LanguageVersionFacts.TryParse(LanguageVersion, out languageVersion))
            {
                var message = $"Invalid language version '{LanguageVersion}'.";
                return new CompilerResult(false, new[] { message });
            }

            var parseOptions = new CSharpParseOptions(languageVersion);
            var syntaxTrees = sourceFiles.Select(sourceFile =>
            {
                var path = Path.GetFullPath(sourceFile);
                var contents = File.ReadAllBytes(path);
                var sourceText = SourceText.From(contents, contents.Length, checksumAlgorithm: SourceHashAlgorithm.Sha256);
                return CSharpSyntaxTree.ParseText(sourceText, parseOptions, path);
            });
            var referencePaths = references.ToArray();
            var missingReferences = referencePaths.Where(path => !File.Exists(path)).ToArray();
            var metadataReferences = referencePaths
                .Except(missingReferences)
                .Select(path => MetadataReference.CreateFromFile(path));
            var compilationOptions = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: IncludeDebugInformation ? OptimizationLevel.Debug : OptimizationLevel.Release,
                allowUnsafe: AllowUnsafeBlocks);
            var compilation = CSharpCompilation.Create(
                AssemblyName ?? Path.GetFileNameWithoutExtension(outputFileName),
                syntaxTrees,
                metadataReferences,
                compilationOptions);

            using var assemblyStream = new MemoryStream();
            using var symbolsStream = IncludeDebugInformation ? new MemoryStream() : null;
            var symbolsFileName = Path.ChangeExtension(outputFileName, SymbolsExtension);
            var emitOptions = new EmitOptions(
                debugInformationFormat: DebugInformationFormat.PortablePdb,
                pdbFilePath: IncludeDebugInformation ? symbolsFileName : null);
            var emitResult = compilation.Emit(assemblyStream, symbolsStream, options: emitOptions);
            var diagnostics = missingReferences
                .Select(path => $"warning: Metadata file '{path}' could not be found.")
                .Concat(emitResult.Diagnostics
                    .Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning)
                    .Select(diagnostic => diagnostic.ToString()))
                .ToArray();

            if (emitResult.Success)
            {
                File.WriteAllBytes(outputFileName, assemblyStream.ToArray());
                if (symbolsStream is not null)
                    File.WriteAllBytes(symbolsFileName, symbolsStream.ToArray());
            }

            return new CompilerResult(emitResult.Success, diagnostics);
        }
    }
}
