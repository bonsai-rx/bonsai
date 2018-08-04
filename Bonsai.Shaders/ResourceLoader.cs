using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public abstract class ResourceLoader : Transform<ResourceConfigurationCollection, ResourceConfigurationCollection>
    {
        protected internal abstract IEnumerable<IResourceConfiguration> GetResources();

        public IObservable<ResourceConfigurationCollection> Process(IObservable<INativeWindow> source)
        {
            return source.Select(input => new ResourceConfigurationCollection(((ShaderWindow)input).ResourceManager, GetResources()));
        }

        public IObservable<ResourceConfigurationCollection> Process<TEventArgs>(IObservable<EventPattern<INativeWindow, TEventArgs>> source)
        {
            return source.Select(input => new ResourceConfigurationCollection(((ShaderWindow)input.Sender).ResourceManager, GetResources()));
        }

        public IObservable<ResourceConfigurationCollection> Process(IObservable<ResourceManager> source)
        {
            return source.Select(input => new ResourceConfigurationCollection(input, GetResources()));
        }

        public override IObservable<ResourceConfigurationCollection> Process(IObservable<ResourceConfigurationCollection> source)
        {
            return source.Select(input => input.AddRange(GetResources()));
        }
    }
}
