using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public interface IResourceConfiguration
    {
        string Name { get; }

        Type Type { get; }

        IDisposable CreateResource(ResourceManager resourceManager);
    }

    public abstract class ResourceConfiguration<TResource> : IResourceConfiguration where TResource : IDisposable
    {
        [Description("The name of the resource.")]
        public string Name { get; set; }

        Type IResourceConfiguration.Type
        {
            get { return typeof(TResource); }
        }

        IDisposable IResourceConfiguration.CreateResource(ResourceManager resourceManager)
        {
            return CreateResource(resourceManager);
        }

        public abstract TResource CreateResource(ResourceManager resourceManager);

        public override string ToString()
        {
            var name = Name;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else return string.Format("{0} [{1}]", name, typeName);
        }
    }
}
