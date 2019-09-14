using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Resources
{
    public sealed class ResourceManager : IDisposable
    {
        bool disposed;
        readonly Dictionary<ResourceKey, IResourceConfiguration> preload = new Dictionary<ResourceKey, IResourceConfiguration>();
        readonly Dictionary<ResourceKey, IDisposable> resources = new Dictionary<ResourceKey, IDisposable>();

        public IDisposable Load(IEnumerable<IResourceConfiguration> source)
        {
            if (disposed)
            {
                throw new ObjectDisposedException(typeof(ResourceManager).Name);
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
            IDisposable resource;
            if (!resources.TryGetValue(key, out resource))
            {
                IResourceConfiguration configuration;
                if (!preload.TryGetValue(key, out configuration))
                {
                    throw new ArgumentException(string.Format("The {1} resource with name '{0}' was not found.", key.Name, key.Type.Name));
                }

                resource = configuration.CreateResource(this);
                resources.Add(key, resource);
            }

            return resource;
        }

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
