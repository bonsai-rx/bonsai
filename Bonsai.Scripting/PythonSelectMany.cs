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
using IronPython.Runtime;

namespace Bonsai.Scripting
{
    [DefaultProperty("Script")]
    [WorkflowElementCategory(ElementCategory.Combinator)]
    [Description("A Python script used to project each element of the input sequence into an enumerable sequence.")]
    public class PythonSelectMany : SingleArgumentExpressionBuilder
    {
        public PythonSelectMany()
        {
            Script = "@returns(bool)\ndef process(input):\n  yield True";
        }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        [Description("The script that determines how each element is projected into a sequence.")]
        public string Script { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            Action load;
            Action unload;
            var engine = IronPython.Hosting.Python.CreateEngine();
            var scope = engine.CreateScope();
            var script = PythonHelper.ReturnsDecorator + Script;
            var scriptSource = engine.CreateScriptSourceFromString(script);
            scriptSource.Execute(scope);

            Type outputType;
            PythonHelper.TryGetOutputType(scope, PythonHelper.ProcessFunction, out outputType);
            scope.TryGetVariable<Action>(PythonHelper.LoadFunction, out load);
            scope.TryGetVariable<Action>(PythonHelper.UnloadFunction, out unload);

            var source = arguments.Single();
            var observableType = source.Type.GetGenericArguments()[0];
            var scopeExpression = Expression.Constant(scope);
            var selectorType = Expression.GetFuncType(observableType, typeof(PythonGenerator));
            var processExpression = Expression.Call(
                scopeExpression,
                "GetVariable",
                new[] { selectorType },
                Expression.Constant(PythonHelper.ProcessFunction));
            var loadExpression = Expression.Constant(load, typeof(Action));
            var unloadExpression = Expression.Constant(unload, typeof(Action));

            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(
                combinatorExpression,
                "Combine",
                new[] { observableType, outputType },
                source,
                processExpression,
                loadExpression,
                unloadExpression);
        }

        IObservable<TResult> Combine<TSource, TResult>(
            IObservable<TSource> source,
            Func<TSource, PythonGenerator> selector,
            Action load,
            Action unload)
        {
            var result = source.SelectMany(input => selector(input).Cast<TResult>());
            if (unload != null) result = result.Finally(unload);
            if (load != null)
            {
                var observable = result;
                result = Observable.Defer(() =>
                {
                    load();
                    return observable;
                });
            }

            return result;
        }
    }
}
