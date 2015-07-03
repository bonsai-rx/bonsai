using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    static class ShaderExtensions
    {
        public static void SetTextureSlot(this Shader shader, string name, TextureUnit texture)
        {
            var samplerLocation = GL.GetUniformLocation(shader.Program, name);
            if (samplerLocation >= 0)
            {
                GL.Uniform1(samplerLocation, (int)(texture - TextureUnit.Texture0));
            }
        }
    }
}
