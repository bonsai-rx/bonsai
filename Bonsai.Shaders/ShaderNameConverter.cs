using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class ShaderNameConverter : ResourceNameConverter
    {
        public ShaderNameConverter()
            : base(typeof(Shader))
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var values = base.GetStandardValues(context);
            var configurationNames = ShaderManager.LoadConfiguration().Shaders;
            if (configurationNames.Count > 0)
            {
                var shaderNames = configurationNames.Select(configuration => configuration.Name);
                if (values != null) shaderNames = shaderNames.Concat(values.Cast<string>());
                values = new StandardValuesCollection(shaderNames.ToArray());
            }

            return values;
        }
    }
}
