using Bonsai.Resources;
using Bonsai.Shaders.Configuration;
using System.Collections.Generic;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that creates a collection of mesh resources to
    /// be loaded into the resource manager.
    /// </summary>
    [DefaultProperty(nameof(Meshes))]
    [Description("Creates a collection of mesh resources to be loaded into the resource manager.")]
    public class MeshResources : ResourceLoader
    {
        readonly MeshConfigurationCollection meshes = new MeshConfigurationCollection();

        /// <summary>
        /// Gets the collection of mesh resources to be loaded into the resource manager.
        /// </summary>
        [Editor("Bonsai.Shaders.Configuration.Design.MeshConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        [Description("The collection of mesh resources to be loaded into the resource manager.")]
        public MeshConfigurationCollection Meshes
        {
            get { return meshes; }
        }

        /// <inheritdoc/>
        protected override IEnumerable<IResourceConfiguration> GetResources()
        {
            return meshes;
        }
    }
}
