using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that generates a sequence containing an array with the names of
    /// the subdirectories matching the specified search pattern.
    /// </summary>
    [DefaultProperty(nameof(Path))]
    [Description("Returns an array with the names of the subdirectories matching the specified search pattern.")]
    public class GetDirectories : Source<string[]>
    {
        /// <summary>
        /// Gets or sets the relative or absolute path of the directory to search.
        /// </summary>
        [Description("The relative or absolute path of the directory to search.")]
        [Editor("Bonsai.Design.FolderNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path { get; set; } = ".";

        /// <summary>
        /// Gets or sets the search string used to match against the names of subdirectories in the path.
        /// This parameter can contain a combination of valid literal path and wildcard characters
        /// (see <see cref="Directory.GetDirectories(string, string, SearchOption)"/>).
        /// </summary>
        [Description("The search string used to match against the names of subdirectories in the path.")]
        public string SearchPattern { get; set; } = "*";

        /// <summary>
        /// Gets or sets a value specifying whether the search should include only the current directory
        /// or all subdirectories.
        /// </summary>
        [Description("Specifies whether the search should include only the current directory or all subdirectories.")]
        public SearchOption SearchOption { get; set; }

        /// <summary>
        /// Generates an observable sequence containing an array of directory names that match the search
        /// pattern in a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <returns>
        /// An observable sequence containing an array with the full names (including paths) for the directories in
        /// <see cref="Path"/> that match the specified <see cref="SearchPattern"/> and <see cref="SearchOption"/>.
        /// </returns>
        public override IObservable<string[]> Generate()
        {
            return Observable.Defer(() => Observable.Return(Directory.GetDirectories(Path, SearchPattern, SearchOption)));
        }
    }
}
