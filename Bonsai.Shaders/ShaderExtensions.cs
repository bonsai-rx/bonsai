﻿using OpenTK.Graphics.OpenGL4;
using System;

namespace Bonsai.Shaders
{
    static class ShaderExtensions
    {
        public static void SetTextureSlot(this Shader shader, string name, TextureUnit texture)
        {
            var samplerLocation = GL.GetUniformLocation(shader.Program, name);
            if (samplerLocation < 0)
            {
                throw new InvalidOperationException(string.Format(
                    "The uniform variable \"{0}\" was not found in shader \"{1}\".",
                    name,
                    shader.Name));
            }

            GL.Uniform1(samplerLocation, texture - TextureUnit.Texture0);
        }
    }
}
