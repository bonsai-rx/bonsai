using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Linq.Expressions;
using IronPython.Runtime;

namespace Bonsai.Scripting.IronPython
{
    /// <summary>
    /// Represents an operator that uses a Python script to project each element of an
    /// observable sequence into multiple elements.
    /// </summary>
    [DefaultProperty(nameof(Script))]
    [WorkflowElementCategory(ElementCategory.Combinator)]
    [TypeDescriptionProvider(typeof(PythonSelectManyTypeDescriptionProvider))]
    [Description("A Python script used to project each element of the sequence into multiple elements.")]
    public class PythonSelectMany : SingleArgumentExpressionBuilder, IScriptingElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PythonSelectMany"/> class.
        /// </summary>
        public PythonSelectMany()
        {
            Script = "@returns(bool)\ndef process(value):\n  yield True";
        }

        /// <summary>
        /// Gets or sets the name of the python operator.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("The name of the python operator.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description for the python operator.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("A description for the python operator.")]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the script that determines how each element is projected into a sequence of elements.
        /// </summary>
        [Editor("Bonsai.Scripting.Python.Design.PythonScriptEditor, Bonsai.Scripting.Python.Design", DesignTypes.UITypeEditor)]
        [Description("The script that determines how each element is projected into a sequence of elements.")]
        public string Script { get; set; }

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var engine = PythonEngine.Create();
            var scope = engine.CreateScope();
            var script = PythonHelper.ReturnsDecorator + Script;
            var scriptSource = engine.CreateScriptSourceFromString(script);
            scriptSource.Execute(scope);

            var source = arguments.Single();
            var observableType = source.Type.GetGenericArguments()[0];
            if (PythonHelper.TryGetClass(scope, "SelectMany", out object selectMany))
            {
                var classExpression = Expression.Constant(selectMany);
                var opExpression = Expression.Constant(engine.Operations);
                var outputType = PythonHelper.GetOutputType(engine.Operations, selectMany, PythonHelper.ProcessFunction);
                return Expression.Call(
                    typeof(PythonTransform),
                    nameof(Process),
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
                    nameof(Process),
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
                processor.Load?.Invoke();
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
                    "A Python script used to project each element of the sequence into multiple elements.");
            }
        }
    }
}
