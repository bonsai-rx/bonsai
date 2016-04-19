using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public abstract class ResourceConfiguration<TResource> where TResource : IDisposable
    {
        [Description("The name of the resource.")]
        public string Name { get; set; }

        public abstract TResource CreateResource();

        public override string ToString()
        {
            var name = Name;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else return string.Format("{0} [{1}]", name, typeName);
        }
    }
}
