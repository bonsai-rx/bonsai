using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that returns the extension of each path string in the sequence.
    /// </summary>
    [Description("Returns the extension of each path string in the sequence.")]
    public class GetExtension : Transform<string, string>
    {
        /// <summary>
        /// Returns the extension of each path string in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="string"/> values from which to get the extension.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="string"/> values representing the extension of each
        /// path in the original sequence, or <see langword="null"/>
        /// (see <see cref="Path.GetExtension(string)"/>).
        /// </returns>
        public override IObservable<string> Process(IObservable<string> source)
        {
            return source.Select(Path.GetExtension);
        }
    }
}
