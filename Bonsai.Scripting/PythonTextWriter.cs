using System;
using System.Text;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using System.IO;
using Bonsai.IO;

namespace Bonsai.Scripting
{
    /// <summary>
    /// Represents an operator that uses a Python script to write each element
    /// of the sequence to a text file.
    /// </summary>
    [Obsolete]
    [Description("A Python script used to write each element of the sequence to a text file.")]
    public class PythonTextWriter : PythonSink
    {
        StreamWriter writer;

        /// <summary>
        /// Gets or sets the name of the output file.
        /// </summary>
        [Description("The name of the output file.")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to append or overwrite the
        /// specified file.
        /// </summary>
        [Description("Indicates whether to append or overwrite the specified file.")]
        public bool Append { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the optional suffix used to generate
        /// file names.
        /// </summary>
        [Description("Specifies the optional suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        /// <inheritdoc/>
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
