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
    public class PythonProjection : CombinatorExpressionBuilder
    {
        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        public string Script { get; set; }

        public override Expression Build()
        {
            var engine = IronPython.Hosting.Python.CreateEngine();
            var scope = engine.CreateScope();
            var scriptSource = engine.CreateScriptSourceFromString(Script);
            scriptSource.Execute(scope);

            Type outputType;
            Func<Type> getOutputType;
            if (scope.TryGetVariable<Func<Type>>("getOutputType", out getOutputType))
            {
                outputType = getOutputType();
            }
            else outputType = typeof(object);

            var observableType = Source.Type.GetGenericArguments()[0];
            var scopeExpression = Expression.Constant(scope);
            var selectorType = Expression.GetFuncType(observableType, outputType);
            var processExpression = Expression.Call(scopeExpression, "GetVariable", new[] { selectorType }, Expression.Constant("process"));

            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", new[] { observableType, outputType }, Source, processExpression);
        }

        IObservable<TResult> Combine<TSource, TResult>(IObservable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(selector);
        }
    }
}
