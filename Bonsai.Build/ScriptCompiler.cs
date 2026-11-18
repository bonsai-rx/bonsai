using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Bonsai.Build
{
    static class ScriptCompiler
    {
        internal const int SuccessExitCode = 0;
        internal const int ErrorExitCode = 1;

        internal static int Run(string[] args, string baseDirectory, TextWriter output)
        {
            var arguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory, sdkDirectory: null);
            foreach (var diagnostic in arguments.Errors)
                output.WriteLine(diagnostic);
            if (arguments.Errors.Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
                return ErrorExitCode;

            if (arguments.OutputFileName is null)
            {
                output.WriteLine("error: An output file must be specified.");
                return ErrorExitCode;
            }

            SyntaxTree[] syntaxTrees;
            try
            {
                syntaxTrees = arguments.SourceFiles.Select(sourceFile =>
                {
                    using var stream = File.OpenRead(sourceFile.Path);
                    var text = SourceText.From(stream, arguments.Encoding, arguments.ChecksumAlgorithm);
                    return CSharpSyntaxTree.ParseText(text, arguments.ParseOptions, sourceFile.Path);
                }).ToArray();
            }
            catch (IOException ex)
            {
                output.WriteLine($"error: {ex.Message}");
                return ErrorExitCode;
            }

            var missingReferences = arguments.MetadataReferences
                .Where(reference => !File.Exists(reference.Reference))
                .ToArray();
            var metadataReferences = arguments.MetadataReferences
                .Except(missingReferences)
                .Select(reference => MetadataReference.CreateFromFile(reference.Reference));

            var outputPath = Path.Combine(arguments.OutputDirectory, arguments.OutputFileName);
            var compilation = CSharpCompilation.Create(
                arguments.CompilationName ?? Path.GetFileNameWithoutExtension(outputPath),
                syntaxTrees,
                metadataReferences,
                arguments.CompilationOptions);

            using var assemblyStream = new MemoryStream();
            using var symbolsStream = arguments.EmitPdb ? new MemoryStream() : null;
            var symbolsPath = arguments.PdbPath ?? Path.ChangeExtension(outputPath, ".pdb");
            var emitOptions = arguments.EmitOptions.WithPdbFilePath(symbolsPath);
            var emitResult = compilation.Emit(assemblyStream, symbolsStream, options: emitOptions);

            foreach (var reference in missingReferences)
                output.WriteLine($"warning: Metadata file '{reference.Reference}' could not be found.");
            foreach (var diagnostic in emitResult.Diagnostics.Where(diagnostic => diagnostic.Severity >= DiagnosticSeverity.Warning))
                output.WriteLine(diagnostic);

            if (!emitResult.Success)
                return ErrorExitCode;

            File.WriteAllBytes(outputPath, assemblyStream.ToArray());
            if (symbolsStream is not null)
                File.WriteAllBytes(symbolsPath, symbolsStream.ToArray());
            return SuccessExitCode;
        }
    }
}
