using Bonsai.Resources;
using Bonsai.Shaders.Configuration;
using System.Collections.Generic;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a collection of texture resources to
    /// be loaded into the resource manager.
    /// </summary>
    [DefaultProperty(nameof(Textures))]
    [Description("Creates a collection of texture resources to be loaded into the resource manager.")]
    public class TextureResources : ResourceLoader
    {
        readonly TextureConfigurationCollection textures = new TextureConfigurationCollection();

        /// <summary>
        /// Gets the collection of texture resources to be loaded into the resource manager.
        /// </summary>
        [Editor("Bonsai.Shaders.Configuration.Design.TextureConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        [Description("The collection of texture resources to be loaded into the resource manager.")]
        public TextureConfigurationCollection Textures
        {
            get { return textures; }
        }

        /// <inheritdoc/>
        protected override IEnumerable<IResourceConfiguration> GetResources()
        {
            return textures;
        }
    }
}
