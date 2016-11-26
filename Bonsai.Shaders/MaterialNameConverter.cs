using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class MaterialNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var materialNames = ShaderManager.LoadConfiguration().Materials.Select(configuration => configuration.Name);
            return new StandardValuesCollection(materialNames.ToArray());
        }
    }
}
