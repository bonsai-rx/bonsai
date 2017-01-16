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

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
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
