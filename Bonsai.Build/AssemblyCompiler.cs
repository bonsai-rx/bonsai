using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        /// Compiles the specified source files into a library assembly.
        /// </summary>
        /// <param name="sources">The source files to compile.</param>
        /// <param name="references">The paths of the assemblies referenced by the source files.</param>
        /// <param name="outputFileName">The path of the assembly file to write.</param>
        /// <returns>
        /// A <see cref="CompilerResult"/> describing whether the compilation succeeded, and the
        /// errors and warnings reported by the compiler. The output files are written only if the
        /// compilation succeeds.
        /// </returns>
        public CompilerResult Compile(
            IEnumerable<SourceFile> sources,
            IEnumerable<string> references,
            string outputFileName)
        {
            if (sources is null)
                throw new ArgumentNullException(nameof(sources));
            if (references is null)
                throw new ArgumentNullException(nameof(references));
            if (string.IsNullOrEmpty(outputFileName))
                throw new ArgumentException("An output file name must be specified.", nameof(outputFileName));

            var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
            var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(
                SourceText.From(source.Text, Encoding.UTF8),
                parseOptions,
                source.Path));
            var metadataReferences = references.Select(path => MetadataReference.CreateFromFile(path));
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
            var diagnostics = emitResult.Diagnostics
                .Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning)
                .Select(diagnostic => diagnostic.ToString())
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
