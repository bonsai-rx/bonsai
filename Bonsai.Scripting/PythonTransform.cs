using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Microsoft.Scripting.Hosting;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Linq.Expressions;

namespace Bonsai.Scripting
{
    /// <summary>
    /// Represents an operator that uses a Python script to transform each
    /// element of an observable sequence.
    /// </summary>
    [DefaultProperty(nameof(Script))]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [TypeDescriptionProvider(typeof(PythonTransformTypeDescriptionProvider))]
    [Description("A Python script used to transform each element of the sequence.")]
    public class PythonTransform : SingleArgumentExpressionBuilder, IScriptingElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PythonTransform"/> class.
        /// </summary>
        public PythonTransform()
        {
            Script = "@returns(bool)\ndef process(value):\n  return True";
        }

        /// <summary>
        /// Gets or sets the name of the python transform.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("The name of the python transform.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description for the python transform.
        /// </summary>
        [Category("Design")]
        [Externalizable(false)]
        [Description("A description for the python transform.")]
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the script that determines the operation of the transform.
        /// </summary>
        [Editor("Bonsai.Scripting.PythonScriptEditor, Bonsai.Scripting", DesignTypes.UITypeEditor)]
        [Description("The script that determines the operation of the transform.")]
        public string Script { get; set; }

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var engine = PythonEngine.Create();
            var scope = engine.CreateScope();
            var script = PythonHelper.ReturnsDecorator + Script;
            var scriptSource = engine.CreateScriptSourceFromString(script);
            scriptSource.Execute(scope);

            object transform;
            var source = arguments.Single();
            var observableType = source.Type.GetGenericArguments()[0];
            if (PythonHelper.TryGetClass(scope, "Transform", out transform))
            {
                var classExpression = Expression.Constant(transform);
                var opExpression = Expression.Constant(engine.Operations);
                var outputType = PythonHelper.GetOutputType(engine.Operations, transform, PythonHelper.ProcessFunction);
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
                    typeof(PythonTransform),
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
                var processor = new PythonProcessor<TSource, TResult>(op, processorClass);
                var result = source.Select(processor.Process);
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
            var processor = new PythonProcessor<TSource, TResult>(scope);
            var result = source.Select(processor.Process);
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

        class PythonTransformTypeDescriptionProvider : TypeDescriptionProvider
        {
            static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(PythonTransform));

            public PythonTransformTypeDescriptionProvider()
                : base(parentProvider)
            {
            }

            public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
            {
                return new ScriptingElementTypeDescriptor(instance,
                    "A Python script used to transform each element of the sequence.");
            }
        }
    }
}
