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
    public class WriteAllText : Sink<string>
    {
        /// <summary>
        /// Gets or sets the relative or absolute path of the file to open for writing.
        /// </summary>
        [Description("The relative or absolute path of the file to open for writing. If the specified file already exists, it is overwritten.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path { get; set; }

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
            return source.Do(contents =>
            {
                using var writer = new StreamWriter(path, append: false);
                writer.Write(contents);
            });
        }
    }
}
