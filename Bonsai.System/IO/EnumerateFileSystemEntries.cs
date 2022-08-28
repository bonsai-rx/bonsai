using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that generates a sequence of file names and directory names matching
    /// the specified search pattern.
    /// </summary>
    [DefaultProperty(nameof(Path))]
    [Description("Generates a sequence of file names and directory names matching the specified search pattern.")]
    public class EnumerateFileSystemEntries : Source<string>
    {
        /// <summary>
        /// Gets or sets the relative or absolute path of the directory to search.
        /// </summary>
        [Description("The relative or absolute path of the directory to search.")]
        [Editor("Bonsai.Design.FolderNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path { get; set; } = ".";

        /// <summary>
        /// Gets or sets the search string used to match against the names of file-system entries in the path.
        /// This parameter can contain a combination of valid literal path and wildcard characters
        /// (see <see cref="Directory.EnumerateFileSystemEntries(string, string, SearchOption)"/>).
        /// </summary>
        [Description("The search string used to match against the names of file-system entries in the path.")]
        public string SearchPattern { get; set; } = "*";

        /// <summary>
        /// Gets or sets a value specifying whether the search should include only the current directory
        /// or all subdirectories.
        /// </summary>
        [Description("Specifies whether the search should include only the current directory or all subdirectories.")]
        public SearchOption SearchOption { get; set; }

        /// <summary>
        /// Generates an observable sequence of file names and directory names that match the search
        /// pattern in a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <returns>
        /// An observable sequence containing the full names (including paths) for the file-system entries in
        /// <see cref="Path"/> that match the specified <see cref="SearchPattern"/> and <see cref="SearchOption"/>.
        /// </returns>
        public override IObservable<string> Generate()
        {
            return Directory.EnumerateFileSystemEntries(Path, SearchPattern, SearchOption).ToObservable();
        }
    }
}
