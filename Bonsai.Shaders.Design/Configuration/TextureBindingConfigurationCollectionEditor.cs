using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    public class TextureBindingConfigurationCollectionEditor : DescriptiveCollectionEditor
    {
        const string BaseText = "Bind";

        public TextureBindingConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[] { typeof(TextureBindingConfiguration), typeof(ImageTextureBindingConfiguration) };
        }

        protected override string GetDisplayText(object value)
        {
            var configuration = (TextureBindingConfiguration)value;
            var name = configuration.Name;
            var textureName = configuration.TextureName;
            if (string.IsNullOrEmpty(name))
            {
                return configuration.GetType().Name;
            }
            else if (string.IsNullOrEmpty(textureName))
            {
                return string.Format("{0}({1})", BaseText, name);
            }
            else return string.Format("{0}({1} : {2})", BaseText, name, textureName);
        }
    }
}
