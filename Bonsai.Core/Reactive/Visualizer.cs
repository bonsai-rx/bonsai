using System;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;
using Bonsai.Expressions;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents an expression builder that uses the encapsulated workflow as a visualizer
    /// to an observable sequence without modifying its elements.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    [XmlType(Namespace = Constants.ReactiveXmlNamespace)]
    [Description("Uses the encapsulated workflow as a visualizer to an observable sequence without modifying its elements.")]
    public class Visualizer : Sink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Visualizer"/> class.
        /// </summary>
        public Visualizer()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Visualizer"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public Visualizer(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        internal override MethodInfo GetProcessMethod(params Type[] typeArguments)
        {
            return typeof(Visualizer).GetMethod(nameof(Process), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(typeArguments);
        }

        static new IObservable<TSource> Process<TSource, TSink>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TSink>> sink)
        {
            return Sink.Process(source, sink);
        }
    }
}
