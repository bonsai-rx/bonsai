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
            return value.ToString();
        }
    }
}
