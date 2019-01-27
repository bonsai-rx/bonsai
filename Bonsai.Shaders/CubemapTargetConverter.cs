using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class CubemapTargetConverter : EnumConverter
    {
        public CubemapTargetConverter()
            : base(typeof(TextureTarget))
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var result = base.ConvertTo(context, culture, value, destinationType);
            if (destinationType == typeof(string))
            {
                result = ((string)result).Substring(TextureTarget.TextureCubeMap.ToString().Length);
            }

            return result;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var text = value as string;
            if (text != null)
            {
                value = TextureTarget.TextureCubeMap.ToString() + text;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new[]
            {
                TextureTarget.TextureCubeMapNegativeX,
                TextureTarget.TextureCubeMapNegativeY,
                TextureTarget.TextureCubeMapNegativeZ,
                TextureTarget.TextureCubeMapPositiveX,
                TextureTarget.TextureCubeMapPositiveY,
                TextureTarget.TextureCubeMapPositiveZ
            });
        }
    }
}
