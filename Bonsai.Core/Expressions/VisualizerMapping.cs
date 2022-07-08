using System;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents the assignment of a specified input source and its corresponding
    /// visualizer to a workflow element.
    /// </summary>
    public sealed class VisualizerMapping
    {
        internal VisualizerMapping(InspectBuilder source, Type visualizerType)
        {
            Source = source;
            VisualizerType = visualizerType;
        }

        /// <summary>
        /// Gets the source of runtime notifications to be visualized.
        /// </summary>
        public InspectBuilder Source { get; }

        /// <summary>
        /// Gets the type of the visualizer used to display notifications from
        /// the source.
        /// </summary>
        public Type VisualizerType { get; }
    }
}
