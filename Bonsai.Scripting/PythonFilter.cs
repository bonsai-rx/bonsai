using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using System.Drawing.Design;

namespace Bonsai.Scripting
{
    public class PythonFilter : Filter<object>
    {
        ScriptEngine engine;
        CompiledCode script;
        ScriptScope scope;

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        public string Script { get; set; }

        public override bool Process(object input)
        {
            scope.SetVariable("input", input);
            script.Execute(scope);

            bool result;
            scope.TryGetVariable("result", out result);
            return result;
        }

        public override IDisposable Load()
        {
            engine = IronPython.Hosting.Python.CreateEngine();
            var source = engine.CreateScriptSourceFromString(Script);
            script = source.Compile();
            scope = engine.CreateScope();
            return base.Load();
        }
    }
}
