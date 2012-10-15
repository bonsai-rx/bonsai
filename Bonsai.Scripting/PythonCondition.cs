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
    [Description("A Python script used to determine which elements of the input sequence are accepted.")]
    public class PythonCondition : CombinatorExpressionBuilder
    {
        public PythonCondition()
        {
            Script = "def process(input):\n    return True";
        }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        [Description("The script that determines the criteria for the condition.")]
        public string Script { get; set; }

        public override Expression Build()
        {
            var engine = IronPython.Hosting.Python.CreateEngine();
            var scope = engine.CreateScope();
            var scriptSource = engine.CreateScriptSourceFromString(Script);
            scriptSource.Execute(scope);

            var observableType = Source.Type.GetGenericArguments()[0];
            var scopeExpression = Expression.Constant(scope);
            var predicateType = Expression.GetFuncType(observableType, typeof(bool));
            var processExpression = Expression.Call(scopeExpression, "GetVariable", new[] { predicateType }, Expression.Constant("process"));

            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", new[] { observableType }, Source, processExpression);
        }

        IObservable<TSource> Combine<TSource>(IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Where(predicate);
        }
    }
}
