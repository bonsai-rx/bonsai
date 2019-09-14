using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Resources
{
    public class ResourceConfigurationCollection : IEnumerable<IResourceConfiguration>
    {
        readonly ResourceManager manager;
        readonly IEnumerable<IResourceConfiguration> resources;

        public ResourceConfigurationCollection(ResourceManager resourceManager, IEnumerable<IResourceConfiguration> collection)
        {
            if (resourceManager == null)
            {
                throw new ArgumentNullException("resourceManager");
            }

            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            manager = resourceManager;
            resources = collection;
        }

        public ResourceManager ResourceManager
        {
            get { return manager; }
        }

        public ResourceConfigurationCollection Add(IResourceConfiguration resource)
        {
            return AddRange(Enumerable.Repeat(resource, 1));
        }

        public ResourceConfigurationCollection AddRange(IEnumerable<IResourceConfiguration> collection)
        {
            return new ResourceConfigurationCollection(manager, resources.Concat(collection));
        }

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
