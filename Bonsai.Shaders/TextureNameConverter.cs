using Bonsai.Resources;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Shaders
{
    class TextureNameConverter : ResourceNameConverter
    {
        public TextureNameConverter()
            : base(typeof(Texture))
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var values = base.GetStandardValues(context);
            var configurationResources = ShaderManager.LoadConfiguration().Textures;
            if (configurationResources.Count > 0)
            {
                var textureNames = configurationResources.Select(configuration => configuration.Name);
                if (values != null) textureNames = textureNames.Concat(values.Cast<string>());
                values = new StandardValuesCollection(textureNames.ToArray());
            }

            return values;
        }
    }
}
