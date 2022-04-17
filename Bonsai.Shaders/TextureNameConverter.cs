using Bonsai.Resources;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Shaders
{
    public class TextureNameConverter : ResourceNameConverter
    {
        public TextureNameConverter()
            : base(typeof(Texture))
        {
        }

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
