using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using System.Drawing.Design;
using System.Threading.Tasks;

namespace Bonsai.Scripting
{
    public class PythonSink : Sink<object>
    {
        Task scriptTask;
        ScriptEngine engine;
        CompiledCode script;
        ScriptScope scope;

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        public string LoadScript { get; set; }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        public string Script { get; set; }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        public string UnloadScript { get; set; }

        public override void Process(object input)
        {
            if (scriptTask == null) return;

            scriptTask = scriptTask.ContinueWith(task =>
            {
                scope.SetVariable("input", input);
                script.Execute(scope);
            });
        }

        protected virtual ScriptEngine CreateEngine()
        {
            return IronPython.Hosting.Python.CreateEngine();
        }

        public override IDisposable Load()
        {
            scriptTask = new Task(() => { });
            scriptTask.Start();

            engine = CreateEngine();
            scope = engine.CreateScope();
            if (!string.IsNullOrEmpty(LoadScript))
            {
                engine.Execute(LoadScript, scope);
            }

            var source = engine.CreateScriptSourceFromString(Script);
            script = source.Compile();
            return base.Load();
        }

        protected override void Unload()
        {
            scriptTask = scriptTask.ContinueWith(task =>
            {
                if (!string.IsNullOrEmpty(UnloadScript))
                {
                    engine.Execute(UnloadScript, scope);
                }
            });

            scriptTask.Wait();
            base.Unload();
        }
    }
}
