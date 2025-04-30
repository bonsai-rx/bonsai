using System.ComponentModel;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents expression builder instances that map observable sequences to
    /// a specified visualizer type.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Property)]
    [WorkflowElementIcon("Bonsai:ElementIcon.Visualizer")]
    public abstract class VisualizerMappingExpressionBuilder : SingleArgumentExpressionBuilder, ISerializableElement
    {
        internal VisualizerMappingExpressionBuilder()
        {
        }

        /// <summary>
        /// Gets or sets a value specifying the visualizer type to be used by the operator.
        /// </summary>
        [Externalizable(false)]
        [Description("Specifies the visualizer type to be used by the operator.")]
        public TypeMapping VisualizerType { get; set; }

        object ISerializableElement.Element
        {
            get { return VisualizerType; }
        }
    }
}
