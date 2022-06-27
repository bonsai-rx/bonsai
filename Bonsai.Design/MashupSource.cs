using Bonsai.Expressions;
using System;

namespace Bonsai.Design
{
    /// <summary>
    /// Represents an association between a workflow element and a type visualizer
    /// to be combined in a <see cref="MashupVisualizer"/>.
    /// </summary>
    public class MashupSource : ITypeVisualizerContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MashupSource"/> class
        /// using the specified source and type visualizer.
        /// </summary>
        /// <param name="source">The source of runtime notifications to be visualized.</param>
        /// <param name="visualizer">
        /// The type visualizer used to display notifications from the
        /// <paramref name="source"/> in the context of the mashup combination.
        /// </param>
        public MashupSource(InspectBuilder source, DialogTypeVisualizer visualizer)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
        }

        /// <summary>
        /// Gets the source of runtime notifications to be visualized.
        /// </summary>
        public InspectBuilder Source { get; }

        /// <summary>
        /// Gets the type visualizer used to display notifications from the source
        /// in the context of the mashup combination.
        /// </summary>
        public DialogTypeVisualizer Visualizer { get; }
    }

    /// <summary>
    /// Provides a generic type signature which can be used to declare that the
    /// specified mashup visualizer type accepts mashup combinations from any
    /// type visualizer object.
    /// </summary>
    /// <typeparam name="TMashupVisualizer">
    /// The type visualizer class which can be used to combine with any
    /// <see cref="DialogTypeVisualizer"/> instance.
    /// </typeparam>
    public static class MashupSource<TMashupVisualizer>
        where TMashupVisualizer : MashupVisualizer
    {
    }

    /// <summary>
    /// Represents a generic type signature which can be used to declare an association
    /// between the specified mashup visualizer type and compatible type visualizer
    /// objects.
    /// </summary>
    /// <typeparam name="TMashupVisualizer">
    /// The type visualizer which will accept to be combined with <typeparamref name="TVisualizer"/>.
    /// </typeparam>
    /// <typeparam name="TVisualizer">
    /// The type visualizer to be combined with <typeparamref name="TMashupVisualizer"/>.
    /// </typeparam>
    public static class MashupSource<TMashupVisualizer, TVisualizer>
        where TMashupVisualizer : MashupVisualizer
        where TVisualizer : DialogTypeVisualizer
    {
    }
}
