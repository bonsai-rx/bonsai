using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Resources
{
    /// <summary>
    /// Provides the abstract base class for operators that load specific resources into the resource manager.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public abstract class ResourceLoader : Transform<ResourceConfigurationCollection, ResourceConfigurationCollection>
    {
        /// <summary>
        /// Returns the set of resources to be loaded into the resource manager.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="IResourceConfiguration"/> objects to be loaded
        /// into the resource manager.
        /// </returns>
        protected internal abstract IEnumerable<IResourceConfiguration> GetResources();

        /// <summary>
        /// Bundles a set of resources to be loaded into the resource manager.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="ResourceManager"/> objects onto which the resources
        /// will be loaded.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ResourceConfigurationCollection"/> objects which
        /// can be used to load resources into the resource manager.
        /// </returns>
        public IObservable<ResourceConfigurationCollection> Process(IObservable<ResourceManager> source)
        {
            return source.Select(input => new ResourceConfigurationCollection(input, GetResources()));
        }

        /// <summary>
        /// Appends a new set of resources to be loaded into the resource manager.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="ResourceConfigurationCollection"/> objects with which
        /// the loader resources will be combined.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="ResourceConfigurationCollection"/> objects which
        /// can be used to load resources into the resource manager.
        /// </returns>
        public override IObservable<ResourceConfigurationCollection> Process(IObservable<ResourceConfigurationCollection> source)
        {
            return source.Select(input => input.AddRange(GetResources()));
        }
    }
}
