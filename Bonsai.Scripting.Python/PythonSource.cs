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
    [TypeDescriptionProvider(typeof(PythonSourceTypeDescriptionProvider))]
    public class PythonSource : ZeroArgumentExpressionBuilder, IScriptingElement
    {
        public PythonSource()
        {
            Script = "@returns(int)\ndef generate():\n  yield 0";
        }

        /// <summary>
        /// Gets or sets the name of the python source.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("The name of the python source.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description for the python source.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("A description for the python source.")]
        [Editor(DesignTypes.MultilineStringEditor, typeof(UITypeEditor))]
        public string Description { get; set; }

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

        class PythonSourceTypeDescriptionProvider : TypeDescriptionProvider
        {
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(PythonSource));

            public PythonSourceTypeDescriptionProvider()
                : base(parentProvider)
            {
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                return new ScriptingElementTypeDescriptor(instance,
                    "A Python script used to generate individual elements of an observable sequence.");
            }
        }
    }
}
