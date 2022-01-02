using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that returns the file name and extension of each path string in the sequence.
    /// </summary>
    [Description("Returns the file name and extension of each path string in the sequence.")]
    public class GetFileName : Transform<string, string>
    {
        /// <summary>
        /// Returns the file name and extension of each path string in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="string"/> values from which to obtain the file name and extension.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="string"/> values containing the characters after the
        /// last directory character of each path in the original sequence
        /// (see <see cref="Path.GetExtension(string)"/>).
        /// </returns>
        public override IObservable<string> Process(IObservable<string> source)
        {
            return source.Select(Path.GetFileName);
        }
    }
}
