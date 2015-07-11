using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [XmlInclude(typeof(Texture2D))]
    [XmlInclude(typeof(ImageTexture))]
    [XmlInclude(typeof(TextureReference))]
    [XmlInclude(typeof(FramebufferTexture))]
    public abstract class TextureConfiguration
    {
        public string Name { get; set; }

        public abstract void Load(Shader shader);

        public abstract void Bind(Shader shader);

        public abstract void Unbind(Shader shader);

        public abstract void Unload(Shader shader);

        public abstract int GetTexture();

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
