using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that changes the extension of each path string in
    /// the sequence.
    /// </summary>
    [Description("Changes the extension of each path string in the sequence.")]
    public class ChangeExtension : Transform<string, string>
    {
        /// <summary>
        /// Gets or sets the new extension, with or without a leading period. Specify
        /// null to remove any extension from the path.
        /// </summary>
        [Description("The new extension, with or without a leading period. Specify null to remove any extension from the path.")]
        public string Extension { get; set; }

        /// <summary>
        /// Changes the extension of each path string in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="string"/> values for which to change the extension.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="string"/> values representing the path string with
        /// the changed extension, for each path in the original sequence.
        /// </returns>
        public override IObservable<string> Process(IObservable<string> source)
        {
            return source.Select(path => Path.ChangeExtension(path, Extension));
        }
    }
}
