using Bonsai;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// An operator that generates a sequence of strings representing
    /// the path to a user-selected folder.
    /// </summary>
    [DefaultProperty("Path")]
    [Description("Returns a string with the name of the selected folder path.")]
    public class SelectFolder : Source<string>
    {
        /// <summary>
        /// Gets or sets the relative or absolute path of the selected folder.
        /// </summary>
        [Editor("Bonsai.Design.FolderNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The relative or absolute path of the selected folder.")]
        public string Path { get; set; } = ".";

        /// <summary>
        /// Generates an observable sequence containing a string of the selected folder.
        /// </summary>
        /// <returns>
        /// An observable sequence containing a string of the selected folder.
        /// </returns>
        public override IObservable<string> Generate()
        {
            return Observable.Return(Path);
        }

        /// <summary>
        /// Generates an observable sequence containing a string of the selected folder,
        /// triggered with any input sequence.
        /// </summary>
        /// <returns>
        /// An observable sequence containing a string of the selected folder.
        /// </returns>
        public IObservable<string> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => {return Path;});
        }
        }
}
