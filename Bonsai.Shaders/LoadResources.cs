using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Loads a collection of resources into the resource manager.")]
    public class LoadResources : Combinator<ResourceConfigurationCollection, IDisposable>
    {
        public override IObservable<IDisposable> Process(IObservable<ResourceConfigurationCollection> source)
        {
            return source.Select(input => input.ResourceManager.Load(input));
        }

        public IObservable<IDisposable> Process(IObservable<Tuple<IEnumerable<IResourceConfiguration>, ResourceManager>> source)
        {
            return source.Select(input => input.Item2.Load(input.Item1));
        }
    }
}
