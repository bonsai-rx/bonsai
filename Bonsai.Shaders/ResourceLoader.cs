using Bonsai.Resources;
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
    public abstract class ResourceLoader : Bonsai.Resources.ResourceLoader
    {
        public IObservable<ResourceConfigurationCollection> Process(IObservable<INativeWindow> source)
        {
            return source.Select(input => new ResourceConfigurationCollection(((ShaderWindow)input).ResourceManager, GetResources()));
        }

        public IObservable<ResourceConfigurationCollection> Process<TEventArgs>(IObservable<EventPattern<INativeWindow, TEventArgs>> source)
        {
            return source.Select(input => new ResourceConfigurationCollection(((ShaderWindow)input.Sender).ResourceManager, GetResources()));
        }
    }
}
