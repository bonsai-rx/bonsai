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
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("A Python script used to process and convert individual elements of the input sequence.")]
    public class PythonTransform : SingleArgumentExpressionBuilder
    {
        public PythonTransform()
        {
            Script = "import clr\n\ndef getOutputType():\n    return clr.GetClrType(bool)\n\ndef process(input):\n    return True";
        }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        [Description("The script that determines the operation of the transform.")]
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

            var source = arguments.Single();
            var observableType = source.Type.GetGenericArguments()[0];
            var scopeExpression = Expression.Constant(scope);
            var selectorType = Expression.GetFuncType(observableType, outputType);
            var processExpression = Expression.Call(scopeExpression, "GetVariable", new[] { selectorType }, Expression.Constant("process"));

            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", new[] { observableType, outputType }, source, processExpression);
        }

        IObservable<TResult> Combine<TSource, TResult>(IObservable<TSource> source, Func<TSource, TResult> selector)
        {
            return source.Select(selector);
        }
    }
}
