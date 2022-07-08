using Bonsai.Dag;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder specifying an observable sequence to be combined
    /// in a mashup visualizer.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Property)]
    [XmlType("VisualizerMapping", Namespace = Constants.XmlNamespace)]
    [WorkflowElementIcon(typeof(ElementCategory), "ElementIcon.Visualizer")]
    [Description("Specifies an observable sequence to be combined in a mashup visualizer.")]
    public sealed class VisualizerMappingBuilder : SingleArgumentExpressionBuilder, IArgumentBuilder, ISerializableElement
    {
        /// <summary>
        /// Gets or sets an optional type mapping specifying the visualizer type which
        /// the selected properties will be projected into.
        /// </summary>
        [Browsable(false)]
        public TypeMapping VisualizerType { get; set; }

        object ISerializableElement.Element
        {
            get { return VisualizerType; }
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            return arguments.First();
        }

        bool IArgumentBuilder.BuildArgument(Expression source, Edge<ExpressionBuilder, ExpressionBuilderArgument> successor, out Expression argument)
        {
            if (successor.Target.Value is InspectBuilder targetBuilder &&
                InspectBuilder.GetInspectBuilder(source) is InspectBuilder sourceVisualizer)
            {
                targetBuilder.AddVisualizerMapping(successor.Label.Index, sourceVisualizer, VisualizerType?.TargetType);
            }

            argument = source;
            return false;
        }
    }
}
