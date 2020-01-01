using Bonsai.Resources;
using Bonsai.Shaders.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [DefaultProperty("Shaders")]
    [Description("Creates a collection of shader resources to be loaded into the resource manager.")]
    public class ShaderResources : ResourceLoader
    {
        readonly ShaderConfigurationCollection shaders = new ShaderConfigurationCollection();

        [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        [Description("The collection of shader resources to be loaded into the resource manager.")]
        public ShaderConfigurationCollection Shaders
        {
            get { return shaders; }
        }

        protected override IEnumerable<IResourceConfiguration> GetResources()
        {
            return shaders;
        }
    }
}
