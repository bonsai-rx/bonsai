using Bonsai.Resources;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Provides a type converter to convert a mesh name to and from other
    /// representations. It also provides a mechanism to find existing meshes
    /// which have been declared in the workflow.
    /// </summary>
    public class MeshNameConverter : ResourceNameConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MeshNameConverter"/> class.
        /// </summary>
        public MeshNameConverter()
            : base(typeof(Mesh))
        {
        }

        /// <inheritdoc/>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var values = base.GetStandardValues(context);
#pragma warning disable CS0612 // Type or member is obsolete
            var configurationResources = ShaderManager.LoadConfiguration().Meshes;
            if (configurationResources.Count > 0)
            {
                var meshNames = configurationResources.Select(configuration => configuration.Name);
                if (values != null) meshNames = meshNames.Concat(values.Cast<string>());
                values = new StandardValuesCollection(meshNames.ToArray());
            }
#pragma warning restore CS0612 // Type or member is obsolete
            return values;
        }
    }
}
