using Bonsai.Resources;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Shaders
{
    public class MeshNameConverter : ResourceNameConverter
    {
        public MeshNameConverter()
            : base(typeof(Mesh))
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var values = base.GetStandardValues(context);
#pragma warning disable CS0612 // Type or member is obsolete
            var configurationResources = ShaderManager.LoadConfiguration().Meshes;
            if (configurationResources.Count > 0)
            {
                var meshNames = configurationResources.Select(configuration => configuration.Name);
                if (values != null) meshNames = meshNames.Concat(values.Cast<string>());
                values = new StandardValuesCollection(meshNames.ToArray());
            }
#pragma warning restore CS0612 // Type or member is obsolete
            return values;
        }
    }
}
