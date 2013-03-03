using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using System.Drawing.Design;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace Bonsai.Scripting
{
    [Description("A Python script used to operate on individual elements of the input sequence.")]
    public class PythonSink : Sink<object>
    {
        Action load;
        Action unload;
        Action<object> process;
        Task scriptTask;

        public PythonSink()
        {
            Script = "def process(input):\n";
        }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        [Description("The script that determines the operation of the sink.")]
        public string Script { get; set; }

        public override void Process(object input)
        {
            if (scriptTask == null) return;

            var processAction = process;
            if (processAction != null)
            {
                scriptTask = scriptTask.ContinueWith(task =>
                {
                    processAction(input);
                });
            }
        }

        protected virtual ScriptEngine CreateEngine()
        {
            return IronPython.Hosting.Python.CreateEngine();
        }

        public override IDisposable Load()
        {
            scriptTask = new Task(() => { });
            scriptTask.Start();

            var engine = CreateEngine();
            var scope = engine.CreateScope();
            engine.Execute(Script, scope);
            scope.TryGetVariable<Action>("load", out load);
            scope.TryGetVariable<Action>("unload", out unload);
            process = scope.GetVariable<Action<object>>("process");

            if (load != null)
            {
                load();
            }
            return base.Load();
        }

        protected override void Unload()
        {
            var unloadAction = unload;
            if (unloadAction != null)
            {
                scriptTask = scriptTask.ContinueWith(task => unloadAction());
            }

            load = null;
            unload = null;
            process = null;
            scriptTask.Wait();
            base.Unload();
        }
    }
}
