using Bonsai.Audio.Configuration;
using Bonsai.Resources;
using OpenTK.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    [DefaultProperty("Buffers")]
    [Description("Creates a collection of buffer resources and audio sources to be loaded into the resource manager.")]
    public class AudioResources : ResourceLoader
    {
        readonly BufferConfigurationCollection buffers = new BufferConfigurationCollection();
        readonly SourceConfigurationCollection sources = new SourceConfigurationCollection();

        [Editor("Bonsai.Resources.Design.ResourceCollectionEditor, Bonsai.System.Design", typeof(UITypeEditor))]
        [Description("The collection of buffer resources to be loaded into the resource manager.")]
        public BufferConfigurationCollection Buffers
        {
            get { return buffers; }
        }

        [Editor("Bonsai.Resources.Design.CollectionEditor, Bonsai.System.Design", typeof(UITypeEditor))]
        [Description("The collection of audio sources to be loaded into the resource manager.")]
        public SourceConfigurationCollection Sources
        {
            get { return sources; }
        }

        protected override IEnumerable<IResourceConfiguration> GetResources()
        {
            return buffers.Concat<IResourceConfiguration>(sources);
        }

        public IObservable<ResourceConfigurationCollection> Process(IObservable<AudioContext> source)
        {
            return source.Select(context =>
            {
                return new ResourceConfigurationCollection(context.ResourceManager, GetResources());
            });
        }
    }
}
