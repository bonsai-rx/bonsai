using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Linq.Expressions;
using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;

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
