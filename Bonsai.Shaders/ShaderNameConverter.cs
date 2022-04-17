using Bonsai.Resources;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Provides a type converter to convert a shader name to and from other
    /// representations. It also provides a mechanism to find existing shaders
    /// which have been declared in the workflow.
    /// </summary>
    public class ShaderNameConverter : ResourceNameConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderNameConverter"/> class.
        /// </summary>
        public ShaderNameConverter()
            : base(typeof(Shader))
        {
        }

        /// <inheritdoc/>
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
