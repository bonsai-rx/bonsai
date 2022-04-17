using Bonsai.Resources;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Provides a type converter to convert a texture name to and from other
    /// representations. It also provides a mechanism to find existing textures
    /// which have been declared in the workflow.
    /// </summary>
    public class TextureNameConverter : ResourceNameConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextureNameConverter"/> class.
        /// </summary>
        public TextureNameConverter()
            : base(typeof(Texture))
        {
        }

        /// <inheritdoc/>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var values = base.GetStandardValues(context);
#pragma warning disable CS0612 // Type or member is obsolete
            var configurationResources = ShaderManager.LoadConfiguration().Textures;
            if (configurationResources.Count > 0)
            {
                var textureNames = configurationResources.Select(configuration => configuration.Name);
                if (values != null) textureNames = textureNames.Concat(values.Cast<string>());
                values = new StandardValuesCollection(textureNames.ToArray());
            }
#pragma warning restore CS0612 // Type or member is obsolete
            return values;
        }
    }
}
