using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides an abstract base class for type visualizers supporting visualizer mashups.
    /// </summary>
    [Obsolete]
    public abstract class DialogMashupVisualizer : MashupVisualizer
    {
        /// <summary>
        /// Gets the collection of visualizer mashups accepted by this type visualizer.
        /// </summary>
        [Obsolete]
        [XmlIgnore]
        public Collection<VisualizerMashup> Mashups { get; } = new Collection<VisualizerMashup>();
    }
}
