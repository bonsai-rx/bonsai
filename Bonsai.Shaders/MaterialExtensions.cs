using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    static class MaterialExtensions
    {
        public static void SetTextureSlot(this Material material, string name, TextureUnit texture)
        {
            var samplerLocation = GL.GetUniformLocation(material.Program, name);
            if (samplerLocation >= 0)
            {
                GL.Uniform1(samplerLocation, (int)(texture - TextureUnit.Texture0));
            }
        }
    }
}
