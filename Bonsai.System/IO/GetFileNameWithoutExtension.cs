using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that returns the file name without the extension for each
    /// path string in the sequence.
    /// </summary>
    [Description("Returns the file name without the extension for each path string in the sequence.")]
    public class GetFileNameWithoutExtension : Transform<string, string>
    {
        /// <summary>
        /// Returns the file name without the extension for each path string in an observable sequence.
        /// </summary>
        /// <param name="source">A sequence of path <see cref="string"/> values.</param>
        /// <returns>
        /// A sequence of <see cref="string"/> values returned by <see cref="Path.GetFileName(string)"/>,
        /// minus the last period (.) and all characters following it (see
        /// <see cref="Path.GetFileNameWithoutExtension(string)"/>).
        /// </returns>
        public override IObservable<string> Process(IObservable<string> source)
        {
            return source.Select(Path.GetFileNameWithoutExtension);
        }
    }
}
