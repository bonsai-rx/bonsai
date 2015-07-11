using Bonsai.Design;
using OpenTK.Graphics.OpenGL4;
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
                typeof(TexturedModel)
            };
        }

        protected override object CreateInstance(Type itemType)
        {
            var instance = (ShaderConfiguration)base.CreateInstance(itemType);
            if (itemType == typeof(TexturedQuad) || itemType == typeof(TexturedModel))
            {
                instance.TextureUnits.Add(new Texture2D
                {
                    Name = "tex"
                });
            }

            if (itemType == typeof(TexturedModel))
            {
                instance.RenderState.Add(new EnableState { Capability = EnableCap.DepthTest });
                instance.RenderState.Add(new DepthFunctionState { Function = DepthFunction.Less });
            }

            return instance;
        }
    }
}
