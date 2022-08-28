using Bonsai.Expressions;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides additional context information for a <see cref="DialogTypeVisualizer"/>,
    /// such as the workflow element and a source for subscribing to runtime notifications.
    /// </summary>
    public interface ITypeVisualizerContext
    {
        /// <summary>
        /// Gets an <see cref="InspectBuilder"/> object which can be used to subscribe to
        /// runtime notifications and obtain other information about the workflow element
        /// being visualized.
        /// </summary>
        InspectBuilder Source { get; }
    }
}
