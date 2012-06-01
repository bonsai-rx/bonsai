using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting.Hosting;
using System.ComponentModel;
using System.Threading.Tasks;
using System.IO;
using System.Drawing.Design;

namespace Bonsai.Scripting
{
    public class PythonTextWriter : PythonSink
    {
        StreamWriter writer;

        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design",
                "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        public bool Append { get; set; }

        protected override ScriptEngine CreateEngine()
        {
            var engine = base.CreateEngine();
            if (!string.IsNullOrEmpty(FileName))
            {
                writer = new StreamWriter(FileName, Append, Encoding.ASCII);
                engine.Runtime.IO.SetOutput(writer.BaseStream, writer);
            }
            return engine;
        }

        protected override void Unload()
        {
            base.Unload();
            writer.Close();
        }
    }
}
