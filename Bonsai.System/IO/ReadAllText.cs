using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that opens a text file, returns a single string with all
    /// lines in the file, and then closes the file.
    /// </summary>
    [DefaultProperty(nameof(Path))]
    [Description("Opens a text file, returns a single string with all lines in the file, and then closes the file.")]
    public class ReadAllText : Source<string>
    {
        /// <summary>
        /// Gets or sets the relative or absolute path of the file to open for reading.
        /// </summary>
        [Description("The relative or absolute path of the file to open for reading.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path { get; set; }

        /// <summary>
        /// Generates an observable sequence that opens the text file, returns a single string
        /// with all lines in the file, and then closes the file.
        /// </summary>
        /// <returns>
        /// A sequence containing a single string with all lines in the file.
        /// </returns>
        public override IObservable<string> Generate()
        {
            var path = Path;
            return Observable.Defer(() => Observable.Return(File.ReadAllText(path)));
        }
    }
}
