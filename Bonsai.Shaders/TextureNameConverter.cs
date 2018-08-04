using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var configurationNames = ShaderManager.LoadConfiguration().Textures;
            if (configurationNames.Count > 0)
            {
                var textureNames = configurationNames.Select(configuration => configuration.Name);
                if (values != null) textureNames = textureNames.Concat(values.Cast<string>());
                values = new StandardValuesCollection(textureNames.ToArray());
            }

            return values;
        }
    }
}
