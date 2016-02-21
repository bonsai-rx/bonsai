using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    public class TextureConfigurationCollectionEditor : DescriptiveCollectionEditor
    {
        public TextureConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override string GetDisplayText(object value)
        {
            var configuration = (TextureConfiguration)value;
            var text = Convert.ToString(configuration);
            if (!string.IsNullOrEmpty(configuration.Name))
            {
                return string.Format("{0} [{1}]", configuration.Name, text);
            }

            return text;
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                typeof(Texture2D),
                typeof(ImageTexture),
                typeof(TextureReference),
                typeof(FramebufferTexture),
                typeof(FramebufferTextureReference),
            };
        }
    }
}
