namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents expression builder instances that map observable sequences to
    /// a specified visualizer type.
    /// </summary>
    public interface IVisualizerMappingBuilder : IExpressionBuilder
    {
        /// <summary>
        /// Gets or sets a value specifying the visualizer type to be used by the mapping operator.
        /// </summary>
        TypeMapping VisualizerType { get; set; }
    }
}
