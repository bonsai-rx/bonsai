using Bonsai.Audio.Configuration;
using Bonsai.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that creates a collection of buffer resources and audio sources
    /// to be loaded into the resource manager.
    /// </summary>
    [DefaultProperty(nameof(Buffers))]
    [Description("Creates a collection of buffer resources and audio sources to be loaded into the resource manager.")]
    public class AudioResources : ResourceLoader
    {
        readonly BufferConfigurationCollection buffers = new BufferConfigurationCollection();
        readonly SourceConfigurationCollection sources = new SourceConfigurationCollection();

        /// <summary>
        /// Gets the collection of buffer resources to be loaded into the resource manager.
        /// </summary>
        [Editor("Bonsai.Resources.Design.ResourceCollectionEditor, Bonsai.System.Design", DesignTypes.UITypeEditor)]
        [Description("The collection of buffer resources to be loaded into the resource manager.")]
        public BufferConfigurationCollection Buffers
        {
            get { return buffers; }
        }

        /// <summary>
        /// Gets the collection of audio sources to be loaded into the resource manager.
        /// </summary>
        [Editor("Bonsai.Resources.Design.CollectionEditor, Bonsai.System.Design", DesignTypes.UITypeEditor)]
        [Description("The collection of audio sources to be loaded into the resource manager.")]
        public SourceConfigurationCollection Sources
        {
            get { return sources; }
        }

        /// <inheritdoc/>
        protected override IEnumerable<IResourceConfiguration> GetResources()
        {
            return buffers.Concat<IResourceConfiguration>(sources);
        }

        /// <summary>
        /// Creates a collection of buffer resources and audio sources to be loaded into the
        /// resource manager.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="AudioContextManager"/> objects containing the resource managers
        /// into which the resources will be loaded.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ResourceConfigurationCollection"/> objects which
        /// can be used to load resources into the resource manager.
        /// </returns>
        public IObservable<ResourceConfigurationCollection> Process(IObservable<AudioContextManager> source)
        {
            return source.Select(context =>
            {
                return new ResourceConfigurationCollection(context.ResourceManager, GetResources());
            });
        }
    }
}
