using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class TextureReference : TextureConfiguration
    {
        TextureConfiguration textureUnit;

        public TextureReference()
        {
            TextureSlot = TextureUnit.Texture0;
        }

        public TextureUnit TextureSlot { get; set; }

        [Category("Reference")]
        [TypeConverter(typeof(ShaderNameConverter))]
        public string ShaderName { get; set; }

        [Category("Reference")]
        public string TextureName { get; set; }

        public override void Load(Shader shader)
        {
            shader.SetTextureSlot(Name, TextureSlot);
            var referenceShader = shader.Window.Shaders.FirstOrDefault(s => s.Name == ShaderName);
            if (referenceShader == null)
            {
                throw new InvalidOperationException(string.Format(
                    "The shader reference \"{0}\" was not found.",
                    ShaderName));
            }

            shader.Window.Update(() =>
            {
                textureUnit = referenceShader.TextureUnits.FirstOrDefault(t => t.Name == TextureName);
                if (textureUnit == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "The texture unit \"{0}\" was not found in shader program \"{1}\".",
                        TextureName,
                        ShaderName));
                }
            });
        }

        public override void Bind(Shader shader)
        {
            GL.ActiveTexture(TextureSlot);
            GL.BindTexture(TextureTarget.Texture2D, textureUnit.GetTexture());
        }

        public override void Unbind(Shader shader)
        {
            GL.ActiveTexture(TextureSlot);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public override void Unload(Shader shader)
        {
            textureUnit = null;
        }

        public override int GetTexture()
        {
            return textureUnit.GetTexture();
        }

        public override string ToString()
        {
            return string.Format("Reference({0}.{1})", ShaderName, TextureName);
        }
    }
}
