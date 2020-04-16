using System;
using System.Text;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using System.IO;
using Bonsai.IO;

namespace Bonsai.Scripting
{
    [Obsolete]
    [Description("A Python script used to write individual elements of the input sequence to a text file.")]
    public class PythonTextWriter : PythonSink
    {
        StreamWriter writer;

        [Description("The name of the output file.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        [Description("Indicates whether to append or overwrite the specified file.")]
        public bool Append { get; set; }

        [Description("The optional suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        protected override ScriptEngine CreateEngine()
        {
            var engine = base.CreateEngine();
            if (!string.IsNullOrEmpty(FileName))
            {
                var fileName = PathHelper.AppendSuffix(FileName, Suffix);
                writer = new StreamWriter(fileName, Append, Encoding.ASCII);
                engine.Runtime.IO.SetOutput(writer.BaseStream, writer);
            }
            return engine;
        }
    }
}
