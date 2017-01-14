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
    [TypeDescriptionProvider(typeof(PythonSelectManyTypeDescriptionProvider))]
    public class PythonSelectMany : SingleArgumentExpressionBuilder, IScriptingElement
    {
        public PythonSelectMany()
        {
            Script = "@returns(bool)\ndef process(value):\n  yield True";
        }

        /// <summary>
        /// Gets or sets the name of the python script.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("The name of the python script.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description for the python script.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("A description for the python script.")]
        [Editor(DesignTypes.MultilineStringEditor, typeof(UITypeEditor))]
        public string Description { get; set; }

        [Editor(typeof(PythonScriptEditor), typeof(UITypeEditor))]
        [Description("The script that determines how each element is projected into a sequence.")]
        public string Script { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var engine = PythonEngine.Create();
            var scope = engine.CreateScope();
            var script = PythonHelper.ReturnsDecorator + Script;
            var scriptSource = engine.CreateScriptSourceFromString(script);
            scriptSource.Execute(scope);

            object selectMany;
            var source = arguments.Single();
            var observableType = source.Type.GetGenericArguments()[0];
            if (PythonHelper.TryGetClass(scope, "SelectMany", out selectMany))
            {
                var classExpression = Expression.Constant(selectMany);
                var opExpression = Expression.Constant(engine.Operations);
                var outputType = PythonHelper.GetOutputType(engine.Operations, selectMany, PythonHelper.ProcessFunction);
                return Expression.Call(
                    typeof(PythonTransform),
                    "Process",
                    new[] { observableType, outputType },
                    source,
                    opExpression,
                    classExpression);
            }
            else
            {
                var outputType = PythonHelper.GetOutputType(scope, PythonHelper.ProcessFunction);
                var scopeExpression = Expression.Constant(scope);
                return Expression.Call(
                    typeof(PythonSelectMany),
                    "Process",
                    new[] { observableType, outputType },
                    source,
                    scopeExpression);
            }
        }

        static IObservable<TResult> Process<TSource, TResult>(
            IObservable<TSource> source,
            ObjectOperations op,
            object processorClass)
        {
            return Observable.Defer(() =>
            {
                var processor = new PythonProcessor<TSource, PythonGenerator>(op, processorClass);
                var result = source.SelectMany(input => processor.Process(input).Cast<TResult>());
                if (processor.Load != null) processor.Load();
                if (processor.Unload != null)
                {
                    return result.Finally(processor.Unload);
                }
                else return result;
            });
        }

        static IObservable<TResult> Process<TSource, TResult>(
            IObservable<TSource> source,
            ScriptScope scope)
        {
            var processor = new PythonProcessor<TSource, PythonGenerator>(scope);
            var result = source.SelectMany(input => processor.Process(input).Cast<TResult>());
            if (processor.Unload != null)
            {
                result = result.Finally(processor.Unload);
            }

            if (processor.Load != null)
            {
                var observable = result;
                result = Observable.Defer(() =>
                {
                    processor.Load();
                    return observable;
                });
            }

            return result;
        }

        class PythonSelectManyTypeDescriptionProvider : TypeDescriptionProvider
        {
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(PythonSelectMany));

            public PythonSelectManyTypeDescriptionProvider()
                : base(parentProvider)
            {
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                return new ScriptingElementTypeDescriptor(instance,
                    "A Python script used to project each element of the input sequence into an enumerable sequence.");
            }
        }
    }
}
