using System;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Scripting
{
    /// <summary>
    /// Represents an operator that uses a Python script to invoke an action for
    /// each element of an observable sequence.
    /// </summary>
    [Obsolete]
    [DefaultProperty(nameof(Script))]
    [Description("A Python script used to invoke an action for each element of the sequence.")]
    public class PythonSink : Sink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PythonSink"/> class.
        /// </summary>
        public PythonSink()
        {
            Script = "def process(value):\n  return";
        }

        /// <summary>
        /// Gets or sets the script that determines the operation of the sink.
        /// </summary>
        [Editor("Bonsai.Scripting.Python.Design.PythonScriptEditor, Bonsai.Scripting.Python.Design", DesignTypes.UITypeEditor)]
        [Description("The script that determines the operation of the sink.")]
        public string Script { get; set; }

        /// <summary>
        /// Creates an engine for interpreting the Python script.
        /// </summary>
        /// <returns>
        /// An instance of the <see cref="ScriptEngine"/> class used to interpret
        /// the script.
        /// </returns>
        protected virtual ScriptEngine CreateEngine()
        {
            return PythonEngine.Create();
        }

        /// <summary>
        /// Uses a Python script to invoke an action for each element of an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">An observable sequence.</param>
        /// <returns>
        /// An observable sequence that contains the same elements of the
        /// <paramref name="source"/> sequence, with the additional side-effect of
        /// invoking the Python script on each element of the original sequence.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var scriptTask = new Task(() => { });
                scriptTask.Start();

                var engine = CreateEngine();
                var scope = engine.CreateScope();
                engine.Execute(Script, scope);

                PythonProcessor<TSource, object> processor;
                if (PythonHelper.TryGetClass(scope, "Sink", out object sink))
                {
                    processor = new PythonProcessor<TSource, object>(engine.Operations, sink);
                }
                else processor = new PythonProcessor<TSource, object>(scope);

                processor.Load?.Invoke();
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
