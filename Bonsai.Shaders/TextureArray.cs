using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class TextureArray : Texture
    {
        readonly int[] textures;

        public TextureArray(int count)
            : base(0)
        {
            textures = new int[count];
            GL.GenTextures(textures.Length, textures);
        }

        internal int Length
        {
            get { return textures.Length; }
        }

        internal void SetActiveTexture(int index)
        {
            Id = textures[index];
        }

        internal override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteTextures(textures.Length, textures);
            }
        }
    }
}
