using System;
using System.IO;

namespace Bonsai.Build
{
    /// <summary>
    /// Represents the text of a C# source file together with its path.
    /// </summary>
    public sealed class SourceFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceFile"/> class with the specified
        /// path and text.
        /// </summary>
        /// <param name="path">
        /// The path of the source file, which is reported in diagnostics and debug symbols.
        /// </param>
        /// <param name="text">The C# source text.</param>
        public SourceFile(string path, string text)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        /// <summary>
        /// Gets the path of the source file.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the C# source text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Reads the source file at the specified path.
        /// </summary>
        /// <param name="path">The path of the source file to read.</param>
        /// <returns>A <see cref="SourceFile"/> holding the full path and text of the file.</returns>
        public static SourceFile FromFile(string path)
        {
            var fullPath = System.IO.Path.GetFullPath(path);
            return new SourceFile(fullPath, File.ReadAllText(fullPath));
        }
    }
}
