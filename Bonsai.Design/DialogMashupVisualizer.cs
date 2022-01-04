using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Xml.Serialization;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides an abstract base class for type visualizers supporting visualizer mashups.
    /// </summary>
    public abstract class DialogMashupVisualizer : DialogTypeVisualizer
    {
        readonly Collection<VisualizerMashup> mashups = new Collection<VisualizerMashup>();

        /// <summary>
        /// Gets the collection of visualizer mashups accepted by this type visualizer.
        /// </summary>
        [XmlIgnore]
        public Collection<VisualizerMashup> Mashups
        {
            get { return mashups; }
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
        /// Loads type visualizer resources for all the visualizer mashups accepted by
        /// this type visualizer.
        /// </summary>
        /// <param name="provider">
        /// A service provider object which can be used to obtain visualization,
        /// runtime inspection, or other editing services.
        /// </param>
        public virtual void LoadMashups(IServiceProvider provider)
        {
            using (var serviceContainer = new ServiceContainer(provider))
            {
                serviceContainer.AddService(typeof(DialogMashupVisualizer), this);
                foreach (var mashup in mashups)
                {
                    mashup.Visualizer.Load(serviceContainer);
                }
                serviceContainer.RemoveService(typeof(DialogMashupVisualizer));
            }
        }

        /// <summary>
        /// Unloads the resources from all the visualizer mashups accepted by
        /// this type visualizer.
        /// </summary>
        public virtual void UnloadMashups()
        {
            foreach (var mashup in mashups)
            {
                mashup.Visualizer.Unload();
            }
        }
    }
}
