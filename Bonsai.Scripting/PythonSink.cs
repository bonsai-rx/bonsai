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
    public class PythonSink : Sink
    {
        public PythonSink()
        {
            Script = "def process(input):\n";
        }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        [Description("The script that determines the operation of the sink.")]
        public string Script { get; set; }

        protected virtual ScriptEngine CreateEngine()
        {
            return IronPython.Hosting.Python.CreateEngine();
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Create<TSource>(observer =>
            {
                var scriptTask = new Task(() => { });
                scriptTask.Start();

                Action load;
                Action unload;
                var engine = CreateEngine();
                var scope = engine.CreateScope();
                engine.Execute(Script, scope);
                scope.TryGetVariable<Action>("load", out load);
                scope.TryGetVariable<Action>("unload", out unload);
                var processAction = scope.GetVariable<Action<object>>("process");

                if (load != null)
                {
                    load();
                }

                var close = Disposable.Create(() =>
                {
                    var unloadAction = unload;
                    if (unloadAction != null)
                    {
                        scriptTask = scriptTask.ContinueWith(task => unloadAction());
                    }

                    engine.Runtime.IO.OutputWriter.Close();
                    scriptTask.Wait();
                });

                var process = source.Do(input =>
                {
                    if (scriptTask == null) return;

                    if (processAction != null)
                    {
                        scriptTask = scriptTask.ContinueWith(task =>
                        {
                            processAction(input);
                        });
                    }
                }).Subscribe(observer);

                return new CompositeDisposable(process, close);
            });
        }
    }
}
