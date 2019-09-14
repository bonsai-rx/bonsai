using Bonsai.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class MeshNameConverter : ResourceNameConverter
    {
        public MeshNameConverter()
            : base(typeof(Mesh))
        {
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var values = base.GetStandardValues(context);
            var configurationResources = ShaderManager.LoadConfiguration().Meshes;
            if (configurationResources.Count > 0)
            {
                var meshNames = configurationResources.Select(configuration => configuration.Name);
                if (values != null) meshNames = meshNames.Concat(values.Cast<string>());
                values = new StandardValuesCollection(meshNames.ToArray());
            }

            return values;
        }
    }
}
