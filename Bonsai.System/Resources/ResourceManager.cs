using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Resources
{
    /// <summary>
    /// Represents a resource manager that can be used to load and release sets of resources at run time,
    /// and manage their lifespan. Disposing the resource manager will also dispose of any loaded resources.
    /// </summary>
    public sealed class ResourceManager : IDisposable
    {
        bool disposed;
        readonly Dictionary<ResourceKey, IResourceConfiguration> preload = new Dictionary<ResourceKey, IResourceConfiguration>();
        readonly Dictionary<ResourceKey, IDisposable> resources = new Dictionary<ResourceKey, IDisposable>();

        /// <summary>
        /// Loads a set of resources into the resource manager.
        /// </summary>
        /// <param name="source">
        /// A collection of resources to load into the resource manager.
        /// </param>
        /// <returns>
        /// A <see cref="IDisposable"/> object which can be used to unload the
        /// loaded resources.
        /// </returns>
        public IDisposable Load(IEnumerable<IResourceConfiguration> source)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(ResourceManager));
            }

            foreach (var resource in source)
            {
                ResourceKey key;
                key.Name = resource.Name;
                key.Type = resource.Type;
                preload.Add(key, resource);
            }

            var resourceSet = (from configuration in preload
                               select new
                               {
                                   configuration.Key,
                                   Resource = Load(configuration.Value.Type, configuration.Value.Name)
                               }).ToArray();

            preload.Clear();
            return Disposable.Create(() =>
            {
                foreach (var handle in resourceSet)
                {
                    handle.Resource.Dispose();
                    resources.Remove(handle.Key);
                }
            });
        }

        /// <summary>
        /// Loads the resource with the specified name into the resource manager.
        /// </summary>
        /// <typeparam name="TResource">The type of the loaded resource.</typeparam>
        /// <param name="name">The name of the resource to load.</param>
        /// <returns>
        /// The loaded resource. Repeated calls to load the same resource will return
        /// the same object instance.
        /// </returns>
        public TResource Load<TResource>(string name) where TResource : IDisposable
        {
            return (TResource)Load(typeof(TResource), name);
        }

        private IDisposable Load(Type type, string name)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(typeof(ResourceManager).Name);
            }

            ResourceKey key;
            key.Name = name;
            key.Type = type;
            if (!resources.TryGetValue(key, out IDisposable resource))
            {
                if (!preload.TryGetValue(key, out IResourceConfiguration configuration))
                {
                    throw new ArgumentException(string.Format("The {1} resource with name '{0}' was not found.", key.Name, key.Type.Name));
                }

                resource = configuration.CreateResource(this);
                resources.Add(key, resource);
            }

            return resource;
        }

        /// <summary>
        /// Releases all resources used by the <see cref="ResourceManager"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                foreach (var resource in resources.Values)
                {
                    resource.Dispose();
                }

                resources.Clear();
                disposed = true;
            }
        }
    }
}
