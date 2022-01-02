using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that returns the directory information for each path string in the sequence.
    /// </summary>
    [Description("Returns the directory information for each path string in the sequence.")]
    public class GetDirectoryName : Transform<string, string>
    {
        /// <summary>
        /// Returns the directory information for each path string in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="string"/> values representing the path to a file or directory.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="string"/> values representing directory information for each
        /// path in the original sequence, or <see langword="null"/>
        /// (see <see cref="Path.GetDirectoryName(string)"/>).
        /// </returns>
        public override IObservable<string> Process(IObservable<string> source)
        {
            return source.Select(Path.GetDirectoryName);
        }
    }
}
