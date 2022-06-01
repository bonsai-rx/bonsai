namespace Bonsai.Design
{
    /// <summary>
    /// Provides an abstract base class for a visualizer which can be used as
    /// a container for other visualizers.
    /// </summary>
    public abstract class MashupVisualizerContainer : DialogMashupVisualizer
    {
        /// <summary>
        /// Gets the nested mashup located at the specified coordinates.
        /// </summary>
        /// <param name="x">
        /// The x-coordinate used to search for mashups, in absolute screen
        /// coordinates.
        /// </param>
        /// <param name="y">
        /// The y-coordinate used to search for mashups, in absolute screen
        /// coordinates.
        /// </param>
        /// <returns>
        /// The <see cref="MashupTypeVisualizer"/> representing the mashup
        /// located at the specified coordinates, or <see langword="null"/>
        /// if there is no mashup at the specified point.
        /// </returns>
        public abstract MashupTypeVisualizer GetMashupAtPoint(int x, int y);
    }
}
