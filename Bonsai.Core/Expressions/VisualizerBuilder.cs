using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that uses the encapsulated workflow as a visualizer
    /// to an observable sequence without modifying its elements.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    [XmlType("Visualizer", Namespace = Constants.XmlNamespace)]
    [Description("Uses the encapsulated workflow as a visualizer to an observable sequence without modifying its elements.")]
    public class VisualizerBuilder : SinkBuilder
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
