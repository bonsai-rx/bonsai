using System;
using System.Linq;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Scripting.Python
{
    /// <summary>
    /// Represents an operator that uses a Python script to filter the elements
    /// of an observable sequence.
    /// </summary>
    [Obsolete]
    [DefaultProperty(nameof(Script))]
    [WorkflowElementCategory(ElementCategory.Condition)]
    [Description("A Python script used to determine which elements of the input sequence are accepted.")]
    public class PythonCondition : Combinator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PythonCondition"/> class.
        /// </summary>
        public PythonCondition()
        {
            Script = "def process(value):\n  return True";
        }

        /// <summary>
        /// Gets or sets the script that determines the criteria for the condition.
        /// </summary>
        [Editor("Bonsai.Scripting.Python.Design.PythonScriptEditor, Bonsai.Scripting.Python.Design", DesignTypes.UITypeEditor)]
        [Description("The script that determines the criteria for the condition.")]
        public string Script { get; set; }

        /// <summary>
        /// Uses a Python script to filter the elements of an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The observable sequence to filter.
        /// </param>
        /// <returns>
        /// An observable sequence that contains the elements of the <paramref name="source"/>
        /// sequence that satisfy the condition.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var engine = PythonEngine.Create();
                var scope = engine.CreateScope();
                engine.Execute(Script, scope);

                PythonProcessor<TSource, bool> processor;
                if (PythonHelper.TryGetClass(scope, "Condition", out object condition))
                {
                    processor = new PythonProcessor<TSource, bool>(engine.Operations, condition);
                }
                else processor = new PythonProcessor<TSource, bool>(scope);

                if (processor.Load != null)
                {
                    processor.Load();
                }

                var result = source.Where(processor.Process);
                if (processor.Unload != null)
                {
                    result = result.Finally(processor.Unload);
                }

                return result;
            });
        }
    }
}
