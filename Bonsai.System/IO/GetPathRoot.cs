using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that returns the root directory information of each path string in the sequence.
    /// </summary>
    [Description("Returns the root directory information of each path string in the sequence.")]
    public class GetPathRoot : Transform<string, string>
    {
        /// <summary>
        /// Returns the root directory information of each path string in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="string"/> values for which to obtain root directory information.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="string"/> values representing the root directory for each
        /// path in the original sequence (see <see cref="Path.GetPathRoot(string)"/>).
        /// </returns>
        public override IObservable<string> Process(IObservable<string> source)
        {
            return source.Select(Path.GetPathRoot);
        }
    }
}
