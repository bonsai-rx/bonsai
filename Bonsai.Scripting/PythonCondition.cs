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
    [DefaultProperty("Script")]
    [WorkflowElementCategory(ElementCategory.Condition)]
    [Description("A Python script used to determine which elements of the input sequence are accepted.")]
    public class PythonCondition : Combinator
    {
        public PythonCondition()
        {
            Script = "def process(input):\n    return True";
        }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        [Description("The script that determines the criteria for the condition.")]
        public string Script { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                Action load;
                Action unload;
                Func<object, bool> process;
                var engine = IronPython.Hosting.Python.CreateEngine();
                var scope = engine.CreateScope();
                engine.Execute(Script, scope);
                scope.TryGetVariable<Action>("load", out load);
                scope.TryGetVariable<Action>("unload", out unload);
                process = scope.GetVariable<Func<object, bool>>("process");

                if (load != null)
                {
                    load();
                }

                var result = source.Where(input => process(input));
                if (unload != null)
                {
                    result = result.Finally(unload);
                }

                return result;
            });
        }
    }
}
