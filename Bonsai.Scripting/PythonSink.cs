using System;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Scripting
{
    [Obsolete]
    [DefaultProperty("Script")]
    [Description("A Python script used to operate on individual elements of the input sequence.")]
    public class PythonSink : Sink
    {
        public PythonSink()
        {
            Script = "def process(value):\n  return";
        }

        [Editor("Bonsai.Scripting.PythonScriptEditor, Bonsai.Scripting", DesignTypes.UITypeEditor)]
        [Description("The script that determines the operation of the sink.")]
        public string Script { get; set; }

        protected virtual ScriptEngine CreateEngine()
        {
            return PythonEngine.Create();
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var scriptTask = new Task(() => { });
                scriptTask.Start();

                var engine = CreateEngine();
                var scope = engine.CreateScope();
                engine.Execute(Script, scope);

                object sink;
                PythonProcessor<TSource, object> processor;
                if (PythonHelper.TryGetClass(scope, "Sink", out sink))
                {
                    processor = new PythonProcessor<TSource, object>(engine.Operations, sink);
                }
                else processor = new PythonProcessor<TSource, object>(scope);

                if (processor.Load != null)
                {
                    processor.Load();
                }

                return source.Do(input =>
                {
                    scriptTask = scriptTask.ContinueWith(task =>
                    {
                        processor.Process(input);
                    });
                }).Finally(() =>
                {
                    var unloadAction = processor.Unload;
                    if (unloadAction != null)
                    {
                        scriptTask = scriptTask.ContinueWith(task => unloadAction());
                    }

                    engine.Runtime.IO.OutputWriter.Close();
                    scriptTask.Wait();
                });
            });
        }
    }
}
