using System;
using System.Text;
using System.ComponentModel;
using System.IO;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that writes the text representation of each element of the sequence to a file.
    /// </summary>
    [Obsolete]
    [Description("Writes the text representation of each element of the sequence to a file.")]
    public class TextWriter : FileSink<object, StreamWriter>
    {
        /// <summary>
        /// Gets or sets a value indicating whether to append or overwrite the specified file.
        /// </summary>
        [Description("Indicates whether to append or overwrite the specified file.")]
        public bool Append { get; set; }

        /// <inheritdoc/>
        protected override StreamWriter CreateWriter(string fileName, object input)
        {
            return new StreamWriter(fileName, Append, Encoding.ASCII);
        }

        /// <inheritdoc/>
        protected override void Write(StreamWriter writer, object input)
        {
            writer.WriteLine(input);
        }
    }
}
