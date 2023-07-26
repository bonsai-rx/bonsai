using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that opens a text file, writes the source string to
    /// the file, and then closes the file.
    /// </summary>
    [DefaultProperty(nameof(Path))]
    [Description("Creates a new file, writes the source string to the file, and then closes the file.")]
    public class WriteTextFile : Sink<string>
    {
        /// <summary>
        /// Gets or sets the relative or absolute path of the file to open for writing.
        /// </summary>
        [Description("The relative or absolute path of the file to open for writing.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output file should be overwritten if it already exists.
        /// </summary>
        [Description("Indicates whether the output file should be overwritten if it already exists.")]
        public bool Overwrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether text should be appended to the output file if it already exists.
        /// </summary>
        [Description("Indicates whether text should be appended to the output file if it already exists.")]
        public bool Append { get; set; }

        /// <summary>
        /// Creates a new file, writes the string in the observable sequence to the file,
        /// and then closes the file.
        /// </summary>
        /// <param name="source">
        /// The sequence containing the string to write to the file.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the string to the file.
        /// </returns>
        public override IObservable<string> Process(IObservable<string> source)
        {
            var path = Path;
            var overwrite = Overwrite;
            var append = Append;
            return source.Do(contents =>
            {
                PathHelper.EnsureDirectory(path);
                if (File.Exists(path) && !overwrite && !append)
                {
                    throw new IOException($"The file '{path}' already exists.");
                }

                using var writer = new StreamWriter(path, append);
                writer.Write(contents);
            });
        }
    }
}
