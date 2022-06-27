using System.Collections.ObjectModel;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    /// <summary>
    /// Represents a collection of visualizer sources to be combined in a
    /// mashup visualizer.
    /// </summary>
    public class MashupSourceCollection : Collection<MashupSource>
    {
        /// <summary>
        /// Adds a visualizer source to the end of the collection.
        /// </summary>
        /// <param name="source">The source of runtime notifications to be visualized.</param>
        /// <param name="visualizer">
        /// The type visualizer used to display notifications from the
        /// <paramref name="source"/> in the context of the mashup combination.
        /// </param>
        /// <returns>
        /// A <see cref="MashupSource"/> representing the visualizer source
        /// being added to the collection.
        /// </returns>
        public MashupSource Add(InspectBuilder source, DialogTypeVisualizer visualizer)
        {
            var visualizerSource = new MashupSource(source, visualizer);
            Add(visualizerSource);
            return visualizerSource;
        }
    }
}
