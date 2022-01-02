using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonsai.Resources
{
    /// <summary>
    /// Represents an immutable collection of resources to be loaded into a resource manager.
    /// </summary>
    public class ResourceConfigurationCollection : IEnumerable<IResourceConfiguration>
    {
        readonly ResourceManager manager;
        readonly IEnumerable<IResourceConfiguration> resources;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceConfigurationCollection"/> class
        /// using the specified resource manager and a set of resources to load.
        /// </summary>
        /// <param name="resourceManager">The resource manager into which the resources will be loaded.</param>
        /// <param name="collection">The set of resources to be loaded into the resource manager.</param>
        public ResourceConfigurationCollection(ResourceManager resourceManager, IEnumerable<IResourceConfiguration> collection)
        {
            manager = resourceManager ?? throw new ArgumentNullException(nameof(resourceManager));
            resources = collection ?? throw new ArgumentNullException(nameof(collection));
        }

        /// <summary>
        /// Gets the resource manager object into which the resources will be loaded.
        /// </summary>
        public ResourceManager ResourceManager
        {
            get { return manager; }
        }

        /// <summary>
        /// Creates a new collection of resources to be loaded into the resource manager
        /// by appending a new resource to the end of this collection.
        /// </summary>
        /// <param name="resource">
        /// The additional resource to be loaded into the resource manager.
        /// </param>
        /// <returns>
        /// A new <see cref="ResourceConfigurationCollection"/> object containing the
        /// combined set of resources.
        /// </returns>
        public ResourceConfigurationCollection Add(IResourceConfiguration resource)
        {
            return AddRange(Enumerable.Repeat(resource, 1));
        }

        /// <summary>
        /// Creates a new collection of resources to be loaded into the resource manager
        /// by appending a new set of resources to the end of this collection.
        /// </summary>
        /// <param name="collection">
        /// The additional set of resources to be loaded into the resource manager.
        /// </param>
        /// <returns>
        /// A new <see cref="ResourceConfigurationCollection"/> object containing the
        /// combined set of resources.
        /// </returns>
        public ResourceConfigurationCollection AddRange(IEnumerable<IResourceConfiguration> collection)
        {
            return new ResourceConfigurationCollection(manager, resources.Concat(collection));
        }

        /// <inheritdoc/>
        public IEnumerator<IResourceConfiguration> GetEnumerator()
        {
            return resources.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
