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
            Script = "import clr\n\ndef getOutputType():\n    return clr.GetClrType(int)\n\ndef generate():\n    yield 0";
        }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        [Description("The script that determines the operation of the source.")]
        public string Script { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
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

            var scopeExpression = Expression.Constant(scope);
            var generatorType = Expression.GetFuncType(typeof(PythonGenerator));
            var generateExpression = Expression.Call(
                scopeExpression,
                "GetVariable",
                new[] { generatorType },
                Expression.Constant("generate"));

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
