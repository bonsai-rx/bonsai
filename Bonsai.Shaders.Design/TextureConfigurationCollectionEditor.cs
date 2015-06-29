using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Design
{
    public class TextureConfigurationCollectionEditor : DescriptiveCollectionEditor
    {
        public TextureConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                typeof(Texture2D),
                typeof(TextureReference),
                typeof(FramebufferTexture),
            };
        }
    }
}
