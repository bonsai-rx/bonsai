using System;
using System.Linq;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Scripting
{
    [Obsolete]
    [DefaultProperty("Script")]
    [WorkflowElementCategory(ElementCategory.Condition)]
    [Description("A Python script used to determine which elements of the input sequence are accepted.")]
    public class PythonCondition : Combinator
    {
        public PythonCondition()
        {
            Script = "def process(value):\n  return True";
        }

        [Editor("Bonsai.Scripting.PythonScriptEditor, Bonsai.Scripting", DesignTypes.UITypeEditor)]
        [Description("The script that determines the criteria for the condition.")]
        public string Script { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var engine = PythonEngine.Create();
                var scope = engine.CreateScope();
                engine.Execute(Script, scope);

                object condition;
                PythonProcessor<TSource, bool> processor;
                if (PythonHelper.TryGetClass(scope, "Condition", out condition))
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
