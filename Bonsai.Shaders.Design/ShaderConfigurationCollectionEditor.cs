using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Design
{
    class ShaderConfigurationCollectionEditor : DescriptiveCollectionEditor
    {
        public ShaderConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[]
                {
                    typeof(ShaderConfiguration),
                    typeof(TexturedQuad),
                    typeof(TexturedCube)
                };
        }

        protected override object CreateInstance(Type itemType)
        {
            var instance = (ShaderConfiguration)base.CreateInstance(itemType);
            if (itemType == typeof(TexturedQuad))
            {
                instance.TextureUnits.Add(new Texture2D
                {
                    Name = "tex"
                });
            }

            return instance;
        }
    }
}
