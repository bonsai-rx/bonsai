using Bonsai.Resources;
using Bonsai.Shaders.Configuration;
using System.Collections.Generic;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a collection of shader resources to
    /// be loaded into the resource manager.
    /// </summary>
    [DefaultProperty(nameof(Shaders))]
    [Description("Creates a collection of shader resources to be loaded into the resource manager.")]
    public class ShaderResources : ResourceLoader
    {
        readonly ShaderConfigurationCollection shaders = new ShaderConfigurationCollection();

        /// <summary>
        /// Gets the collection of shader resources to be loaded into the resource manager.
        /// </summary>
        [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        [Description("The collection of shader resources to be loaded into the resource manager.")]
        public ShaderConfigurationCollection Shaders
        {
            get { return shaders; }
        }

        /// <inheritdoc/>
        protected override IEnumerable<IResourceConfiguration> GetResources()
        {
            return shaders;
        }
    }
}
