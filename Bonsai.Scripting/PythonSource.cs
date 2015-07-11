﻿using System;
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
using System.Reactive.Concurrency;
using System.Threading.Tasks;

namespace Bonsai.Scripting
{
    [DefaultProperty("Script")]
    [WorkflowElementCategory(ElementCategory.Source)]
    [Description("A Python script used to generate individual elements of an observable sequence.")]
    public class PythonSource : ZeroArgumentExpressionBuilder
    {
        public PythonSource()
        {
            Script = "@returns(int)\ndef generate():\n  yield 0";
        }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        [Description("The script that determines the operation of the source.")]
        public string Script { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var engine = PythonEngine.Create();
            var scope = engine.CreateScope();
            var script = PythonHelper.ReturnsDecorator + Script;
            var scriptSource = engine.CreateScriptSourceFromString(script);
            scriptSource.Execute(scope);

            var scopeExpression = Expression.Constant(scope);
            var outputType = PythonHelper.GetOutputType(scope, PythonHelper.GenerateFunction);
            var generatorType = Expression.GetFuncType(typeof(PythonGenerator));
            var generateExpression = Expression.Call(
                scopeExpression,
                "GetVariable",
                new[] { generatorType },
                Expression.Constant(PythonHelper.GenerateFunction));

            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Generate", new[] { outputType }, generateExpression);
        }

        IObservable<TResult> Generate<TResult>(Func<PythonGenerator> generate)
        {
            return Observable.Create<TResult>(async (observer, token) =>
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var generator = generate();
                        try
                        {
                            foreach (var value in generator.Cast<TResult>())
                            {
                                if (token.IsCancellationRequested) break;
                                observer.OnNext(value);
                            }
                        }
                        finally { generator.close(); }
                        observer.OnCompleted();
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                });
            });
        }
    }
}
