using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class FramebufferTextureReference : TextureConfiguration
    {
        int width;
        int height;
        FramebufferTexture framebufferTexture;

        [Category("Reference")]
        [TypeConverter(typeof(ShaderNameConverter))]
        public string ShaderName { get; set; }

        [Category("Reference")]
        public string TextureName { get; set; }

        public override void Load(Shader shader)
        {
            var referenceShader = shader.Window.Shaders.FirstOrDefault(s => s.Name == ShaderName);
            if (referenceShader == null)
            {
                throw new InvalidOperationException(string.Format(
                    "The shader reference \"{0}\" was not found.",
                    ShaderName));
            }

            shader.Window.Update(() =>
            {
                var textureUnit = referenceShader.TextureUnits.FirstOrDefault(t => t.Name == TextureName);
                if (textureUnit == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "The texture unit \"{0}\" was not found in shader program \"{1}\".",
                        TextureName,
                        ShaderName));
                }

                framebufferTexture = textureUnit as FramebufferTexture;
                if (framebufferTexture == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "The texture unit \"{0}\" in shader program  \"{1}\" is not a framebuffer texture.",
                        TextureName,
                        ShaderName));
                }

                width = framebufferTexture.Width.GetValueOrDefault(shader.Window.Width);
                height = framebufferTexture.Height.GetValueOrDefault(shader.Window.Height);
            });
        }

        public override void Bind(Shader shader)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferTexture.Framebuffer);
            GL.Viewport(0, 0, width, height);
        }

        public override void Unbind(Shader shader)
        {
            GL.Viewport(shader.Window.ClientRectangle);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public override void Unload(Shader shader)
        {
            width = 0;
            height = 0;
            framebufferTexture = null;
        }

        public override int GetTexture()
        {
            return framebufferTexture.GetTexture();
        }

        public override string ToString()
        {
            return string.Format("FramebufferReference({0}.{1})", ShaderName, TextureName);
        }
    }
}
