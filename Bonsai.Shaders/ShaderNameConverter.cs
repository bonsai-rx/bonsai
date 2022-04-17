using Bonsai.Resources;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Shaders
{
    public class ShaderNameConverter : ResourceNameConverter
    {
        public ShaderNameConverter()
            : base(typeof(Shader))
        {
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var values = base.GetStandardValues(context);
#pragma warning disable CS0612 // Type or member is obsolete
            var configurationResources = ShaderManager.LoadConfiguration().Shaders;
            if (configurationResources.Count > 0)
            {
                var shaderNames = configurationResources.Where(IsResourceSupported).Select(configuration => configuration.Name);
                if (values != null) shaderNames = shaderNames.Concat(values.Cast<string>());
                values = new StandardValuesCollection(shaderNames.ToArray());
            }
#pragma warning restore CS0612 // Type or member is obsolete
            return values;
        }
    }
}
