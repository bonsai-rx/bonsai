using System.Text;
using System.ComponentModel;
using System.IO;

namespace Bonsai.IO
{
    [Description("Sinks the text representation of individual elements of the input sequence to a file.")]
    public class TextWriter : FileSink<object, StreamWriter>
    {
        [Description("Indicates whether to append or overwrite the specified file.")]
        public bool Append { get; set; }

        protected override StreamWriter CreateWriter(string fileName, object input)
        {
            return new StreamWriter(fileName, Append, Encoding.ASCII);
        }

        protected override void Write(StreamWriter writer, object input)
        {
            writer.WriteLine(input);
        }
    }
}
