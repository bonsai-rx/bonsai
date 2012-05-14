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
    public class PythonTextWriter : Sink<object>
    {
        Task writerTask;
        StreamWriter writer;

        ScriptEngine engine;
        CompiledCode script;
        ScriptScope scope;

        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design",
                "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        public bool Append { get; set; }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        public string LoadScript { get; set; }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        public string Script { get; set; }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        public string UnloadScript { get; set; }

        public override void Process(object input)
        {
            if (writerTask == null) return;

            writerTask = writerTask.ContinueWith(task =>
            {
                scope.SetVariable("input", input);
                script.Execute(scope);
            });
        }

        public override IDisposable Load()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                writer = new StreamWriter(FileName, Append, Encoding.ASCII);
                writerTask = new Task(() => { });
                writerTask.Start();

                engine = IronPython.Hosting.Python.CreateEngine();
                engine.Runtime.IO.SetOutput(writer.BaseStream, writer);
                scope = engine.CreateScope();
                if (!string.IsNullOrEmpty(LoadScript))
                {
                    engine.Execute(LoadScript, scope);
                }

                var source = engine.CreateScriptSourceFromString(Script);
                script = source.Compile();
            }
            return base.Load();
        }

        protected override void Unload()
        {
            writerTask = writerTask.ContinueWith(task =>
            {
                if (!string.IsNullOrEmpty(UnloadScript))
                {
                    engine.Execute(UnloadScript, scope);
                }
            });

            writerTask.Wait();
            writer.Close();
            base.Unload();
        }
    }
}
