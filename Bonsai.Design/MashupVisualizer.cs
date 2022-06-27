using System;
using System.ComponentModel.Design;
using System.Xml.Serialization;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides an abstract base class for a visualizer which can be combined
    /// with other visualizers.
    /// </summary>
    public abstract class MashupVisualizer : DialogTypeVisualizer
    {
        readonly MashupSourceCollection mashupSources = new MashupSourceCollection();

        /// <summary>
        /// Gets the collection of visualizer sources combined in the mashup visualizer.
        /// </summary>
        [XmlIgnore]
        public MashupSourceCollection MashupSources
        {
            get { return mashupSources; }
        }

        /// <summary>
        /// Loads type visualizer resources using the specified service provider.
        /// </summary>
        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            LoadMashups(provider);
        }

        /// <summary>
        /// Unloads all type visualizer resources.
        /// </summary>
        public override void Unload()
        {
            UnloadMashups();
        }

        /// <summary>
        /// Loads type visualizer resources for all sources combined in the
        /// mashup visualizer.
        /// </summary>
        /// <param name="provider">
        /// A service provider object which can be used to obtain visualization,
        /// runtime inspection, or other editing services.
        /// </param>
        public virtual void LoadMashups(IServiceProvider provider)
        {
            using (var serviceContainer = new ServiceContainer(provider))
            {
                serviceContainer.AddService(typeof(MashupVisualizer), this);
                foreach (var source in mashupSources)
                {
                    source.Visualizer.Load(serviceContainer);
                }
                serviceContainer.RemoveService(typeof(MashupVisualizer));
            }
        }

        /// <summary>
        /// Unloads resources for all sources combined in the mashup visualizer.
        /// </summary>
        public virtual void UnloadMashups()
        {
            foreach (var source in mashupSources)
            {
                source.Visualizer.Unload();
            }
        }

        /// <summary>
        /// Finds the mashup source located at the specified coordinates.
        /// </summary>
        /// <param name="x">
        /// The x-coordinate used to search, in absolute screen coordinates.
        /// </param>
        /// <param name="y">
        /// The y-coordinate used to search, in absolute screen coordinates.
        /// </param>
        /// <returns>
        /// The <see cref="MashupSource"/> representing the mashup source
        /// located at the specified coordinates, or <see langword="null"/>
        /// if there is no source at the specified point.
        /// </returns>
        public virtual MashupSource GetMashupSource(int x, int y)
        {
            return null;
        }
    }
}
