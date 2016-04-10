using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class MeshNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var textureNames = ShaderManager.LoadConfiguration().Meshes.Select(configuration => configuration.Name);
            return new StandardValuesCollection(textureNames.ToArray());
        }
    }
}
