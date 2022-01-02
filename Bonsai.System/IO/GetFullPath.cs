using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that returns the absolute path for each path string in the sequence.
    /// </summary>
    [Description("Returns the absolute path for each path string in the sequence.")]
    public class GetFullPath : Transform<string, string>
    {
        /// <summary>
        /// Represents an operator that returns the absolute path for each path string in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="string"/> values for which to obtain absolute path information.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="string"/> values representing the fully qualified location for each
        /// path in the original sequence (see <see cref="Path.GetFullPath(string)"/>).
        /// </returns>
        public override IObservable<string> Process(IObservable<string> source)
        {
            return source.Select(Path.GetFullPath);
        }
    }
}
