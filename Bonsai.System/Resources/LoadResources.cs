using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Resources
{
    /// <summary>
    /// Represents an operator that loads a collection of resources into the
    /// resource manager.
    /// </summary>
    [Description("Loads a collection of resources into the resource manager.")]
    public class LoadResources : Combinator<ResourceConfigurationCollection, IDisposable>
    {
        /// <summary>
        /// Loads a collection of resources into the resource manager.
        /// </summary>
        /// <param name="source">
        /// The sequence containing the collection of resources to be loaded in the
        /// resource manager.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IDisposable"/> objects which can be used to unload
        /// the loaded resources.
        /// </returns>
        public override IObservable<IDisposable> Process(IObservable<ResourceConfigurationCollection> source)
        {
            return source.Select(input => input.ResourceManager.Load(input));
        }
    }
}
