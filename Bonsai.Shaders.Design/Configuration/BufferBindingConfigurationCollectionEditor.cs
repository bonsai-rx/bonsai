using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    public class BufferBindingConfigurationCollectionEditor : DescriptiveCollectionEditor
    {
        const string BaseText = "Bind";

        public BufferBindingConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                typeof(TextureBindingConfiguration),
                typeof(ImageTextureBindingConfiguration),
                typeof(MeshBindingConfiguration)
            };
        }

        protected override string GetDisplayText(object value)
        {
            var referenceName = string.Empty;
            var configuration = (BufferBindingConfiguration)value;
            var textureBinding = configuration as TextureBindingConfiguration;
            if (textureBinding != null) referenceName = textureBinding.TextureName;
            else
            {
                var meshBinding = configuration as MeshBindingConfiguration;
                if (meshBinding != null) referenceName = meshBinding.MeshName;
            }

            var name = configuration.Name;
            if (string.IsNullOrEmpty(name))
            {
                return configuration.GetType().Name;
            }
            else if (string.IsNullOrEmpty(referenceName))
            {
                return string.Format("{0}({1})", BaseText, name);
            }
            else return string.Format("{0}({1} : {2})", BaseText, name, referenceName);
        }
    }
}
