using Bonsai.Resources;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [Obsolete]
    [Description("Loads a collection of resources into the resource manager.")]
    public class LoadResources : Combinator<ResourceConfigurationCollection, IDisposable>
    {
        public override IObservable<IDisposable> Process(IObservable<ResourceConfigurationCollection> source)
        {
            return source.Select(input => input.ResourceManager.Load(input));
        }
    }
}
