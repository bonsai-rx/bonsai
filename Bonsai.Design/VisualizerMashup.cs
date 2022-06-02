using Bonsai.Expressions;
using System;

namespace Bonsai.Design
{
    /// <summary>
    /// Represents the association between a workflow element and a mashup type visualizer
    /// which can be combined with a <see cref="DialogTypeVisualizer"/>.
    /// </summary>
    [Obsolete]
    public class VisualizerMashup : ITypeVisualizerContext
    {
        readonly InspectBuilder inspectBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizerMashup"/> class
        /// using the specified source and mashup visualizer.
        /// </summary>
        /// <param name="source">The inspector wrapping the workflow element to be visualized.</param>
        /// <param name="visualizer">
        /// The type visualizer used to display notifications from the
        /// <paramref name="source"/> in the context of the mashup combination.
        /// </param>
        public VisualizerMashup(InspectBuilder source, MashupTypeVisualizer visualizer)
        {
            inspectBuilder = source ?? throw new ArgumentNullException(nameof(source));
            Visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
#pragma warning disable CS0612 // Type or member is obsolete
            Source = inspectBuilder.Output;
#pragma warning restore CS0612 // Type or member is obsolete
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizerMashup"/> class
        /// using the specified source and mashup visualizer.
        /// </summary>
        /// <param name="source">
        /// The observable sequence generating the notifications to be visualized.
        /// </param>
        /// <param name="visualizer">
        /// The type visualizer used to display notifications from the
        /// <paramref name="source"/> in the context of the mashup combination.
        /// </param>
        [Obsolete]
        public VisualizerMashup(IObservable<IObservable<object>> source, MashupTypeVisualizer visualizer)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
        }

        /// <summary>
        /// Gets the observable sequence generating the notifications to be visualized.
        /// </summary>
        [Obsolete]
        public IObservable<IObservable<object>> Source { get; private set; }

        /// <summary>
        /// Gets the type visualizer used to display notifications from the source
        /// in the context of the mashup combination.
        /// </summary>
        public MashupTypeVisualizer Visualizer { get; private set; }

        InspectBuilder ITypeVisualizerContext.Source => inspectBuilder;
    }

    /// <summary>
    /// Provides a generic type signature which can be used to declare that a
    /// concrete <see cref="DialogMashupVisualizer"/> type accepts mashup combinations
    /// from any <see cref="DialogTypeVisualizer"/> class.
    /// </summary>
    /// <typeparam name="TMashupVisualizer">
    /// The type visualizer which will accept to be combined with any
    /// <see cref="DialogTypeVisualizer"/> class.
    /// </typeparam>
    [Obsolete]
    public static class VisualizerMashup<TMashupVisualizer>
        where TMashupVisualizer : DialogMashupVisualizer
    {
    }

    /// <summary>
    /// Represents a generic type signature which can be used to declare an association
    /// between a concrete <see cref="DialogMashupVisualizer"/> type and a specific
    /// <see cref="DialogTypeVisualizer"/> class.
    /// </summary>
    /// <typeparam name="TMashupVisualizer">
    /// The type visualizer which will accept to be combined with <typeparamref name="TVisualizer"/>.
    /// </typeparam>
    /// <typeparam name="TVisualizer">
    /// The type visualizer which can be combined with <typeparamref name="TMashupVisualizer"/>.
    /// </typeparam>
    [Obsolete]
    public static class VisualizerMashup<TMashupVisualizer, TVisualizer>
        where TMashupVisualizer : DialogMashupVisualizer
        where TVisualizer : DialogTypeVisualizer
    {
    }
}
