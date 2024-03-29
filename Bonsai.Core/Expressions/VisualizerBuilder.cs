﻿using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Visualizer"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Visualizer))]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [XmlType("Visualizer", Namespace = Constants.XmlNamespace)]
    [Description("Uses the encapsulated workflow as a visualizer to an observable sequence without modifying its elements.")]
    public class VisualizerBuilder : Visualizer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizerBuilder"/> class.
        /// </summary>
        public VisualizerBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizerBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public VisualizerBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }
    }
}
