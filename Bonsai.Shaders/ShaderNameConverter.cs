using Bonsai.Resources;
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
            var configurationResources = ShaderManager.LoadConfiguration().Shaders;
            if (configurationResources.Count > 0)
            {
                var shaderNames = configurationResources.Where(IsResourceSupported).Select(configuration => configuration.Name);
                if (values != null) shaderNames = shaderNames.Concat(values.Cast<string>());
                values = new StandardValuesCollection(shaderNames.ToArray());
            }

            return values;
        }
    }
}
